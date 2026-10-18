#import <Foundation/Foundation.h>
#import <Speech/Speech.h>
#import <AVFoundation/AVFoundation.h>
#include <string.h>

static SFSpeechRecognizer *recognizer;
static SFSpeechAudioBufferRecognitionRequest *request;
static SFSpeechRecognitionTask *task;
static AVAudioEngine *engine;
static NSString *transcript;
static int status;
static int generation;
static int errorCode;
static BOOL captureEnded = YES;
static char textBuffer[4096];

static void EndCapture(BOOL cancelTask) {
    if (engine) {
        [engine.inputNode removeTapOnBus:0];
        [engine stop];
        engine = nil;
    }
    if (!captureEnded) {
        [request endAudio];
        if (!cancelTask) [task finish];
        captureEnded = YES;
    }
    if (cancelTask) { [task cancel]; task = nil; request = nil; }
}

static void StartCapture(int token) {
    if (token != generation) return;
    if (!recognizer.supportsOnDeviceRecognition || !recognizer.available) { status = -2; return; }
    if ([SFSpeechRecognizer authorizationStatus] != SFSpeechRecognizerAuthorizationStatusAuthorized) { status = -1; return; }
    if ([AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio] != AVAuthorizationStatusAuthorized) { status = -1; return; }
    EndCapture(YES);
    request = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
    captureEnded = NO;
    request.requiresOnDeviceRecognition = YES;
    request.shouldReportPartialResults = YES;
    request.contextualStrings = @[@"小禾", @"城堡管家", @"过来玩", @"寄礼物", @"公主"];
    engine = [[AVAudioEngine alloc] init];
    AVAudioInputNode *input = engine.inputNode;
    AVAudioFormat *format = [input outputFormatForBus:0];
    if (!format || format.sampleRate <= 0) { status = -3; EndCapture(YES); return; }
    SFSpeechAudioBufferRecognitionRequest *activeRequest = request;
    [input installTapOnBus:0 bufferSize:1024 format:format block:^(AVAudioPCMBuffer *buffer, AVAudioTime *when) {
        [activeRequest appendAudioPCMBuffer:buffer];
    }];
    task = [recognizer recognitionTaskWithRequest:request resultHandler:^(SFSpeechRecognitionResult *result, NSError *error) {
        if (token != generation) return;
        if (result) {
            @synchronized ([NSProcessInfo processInfo]) {
                transcript = result.bestTranscription.formattedString;
                status = result.isFinal ? 2 : captureEnded ? 3 : 1;
            }
            if (result.isFinal) dispatch_async(dispatch_get_main_queue(), ^{ if(token==generation){EndCapture(NO);task=nil;request=nil;} });
        }
        if (error && status != 2) {
            @synchronized ([NSProcessInfo processInfo]) { errorCode = (int)error.code; status = error.code == 1110 ? -4 : -3; }
            dispatch_async(dispatch_get_main_queue(), ^{ if(token==generation)EndCapture(YES); });
        }
    }];
    [engine prepare];
    NSError *error = nil;
    if (![engine startAndReturnError:&error]) { errorCode = (int)error.code; status = -3; EndCapture(YES); return; }
    status = 1;
}

void BL_StartVoice(void) {
    int token;
    @synchronized ([NSProcessInfo processInfo]) {
        token = ++generation;
        status = 0; errorCode = 0; transcript = @""; textBuffer[0] = 0;
    }
    dispatch_async(dispatch_get_main_queue(), ^{
        if (token != generation) return;
        recognizer = [[SFSpeechRecognizer alloc] initWithLocale:[[NSLocale alloc] initWithLocaleIdentifier:@"zh-CN"]];
        if (!recognizer || !recognizer.supportsOnDeviceRecognition) { status = -2; return; }
        [SFSpeechRecognizer requestAuthorization:^(SFSpeechRecognizerAuthorizationStatus result) {
            if(token!=generation)return;
            if (result != SFSpeechRecognizerAuthorizationStatusAuthorized) { status = -1; return; }
            [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
                if(token!=generation)return;
                if (!granted) { status = -1; return; }
                dispatch_async(dispatch_get_main_queue(), ^{ if(token==generation)StartCapture(token); });
            }];
        }];
    });
}

void BL_StopVoice(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (engine) { @synchronized ([NSProcessInfo processInfo]) { status = 3; } EndCapture(NO); }
        else if (status == 0) { generation++; status = -4; }
    });
}

int BL_VoiceStatus(void) { @synchronized ([NSProcessInfo processInfo]) { return status; } }
int BL_VoiceErrorCode(void) { @synchronized ([NSProcessInfo processInfo]) { return errorCode; } }
const char *BL_VoiceText(void) {
    @synchronized ([NSProcessInfo processInfo]) {
        const char *utf = transcript ? transcript.UTF8String : "";
        strncpy(textBuffer, utf, sizeof(textBuffer) - 1);
        textBuffer[sizeof(textBuffer) - 1] = 0;
        return textBuffer;
    }
}
void BL_CancelVoice(void) {
    @synchronized ([NSProcessInfo processInfo]) { generation++; status = 0; transcript = @""; textBuffer[0] = 0; }
    dispatch_async(dispatch_get_main_queue(), ^{ EndCapture(YES); });
}
