using UnityEngine;

namespace Blocklands
{
    public enum CastleGirlMood { Smile, Tears, Angry, Laugh, Shy, Surprised, Calm }
    public class CastleGirlStyle : MonoBehaviour
    {
        public int StyleId {get;private set;}
        public Color DressColor {get;private set;}
        public CastleGirlMood DefaultMood {get;private set;}
        public CastleGirlMood Mood {get;private set;}
        Transform[] faces;
        static readonly Color[] Colors={new Color(.56f,.48f,.85f),new Color(.37f,.76f,.61f),new Color(.4f,.65f,.93f),new Color(.96f,.76f,.33f),new Color(.91f,.43f,.4f),new Color(.35f,.8f,.85f),new Color(.92f,.59f,.3f)};

        public static RoyalArt Make(Transform parent,int index,Sandbox game=null)
        {
            var art=RoyalArt.Girl(parent,true,false);
            art.name="城堡小伙伴 "+(index+1);var style=art.gameObject.AddComponent<CastleGirlStyle>();style.Build(art,index);if(game!=null)art.gameObject.AddComponent<NeighbourSpeech>().Initialize(game,NeighbourSpeechKind.CastleEnvy);return art;
        }
        void Build(RoyalArt art,int index)
        {
            StyleId=Mathf.Abs(index)%Colors.Length;DressColor=Colors[StyleId];DefaultMood=(CastleGirlMood)StyleId;
            Color hair=StyleId%3==0?new Color(.26f,.12f,.1f):StyleId%3==1?new Color(.5f,.31f,.14f):new Color(.12f,.12f,.19f);
            foreach(var renderer in art.GetComponentsInChildren<Renderer>())
            {
                if(renderer.name=="裙摆"||renderer.name=="上衣"||renderer.name=="鞋")renderer.sharedMaterial.color=DressColor;
                if(renderer.name=="腰带")renderer.sharedMaterial.color=new Color(.98f,.95f,.85f);
                if(renderer.name=="头发"||renderer.name=="马尾")renderer.sharedMaterial.color=hair;
                if(renderer.name=="微笑")renderer.gameObject.SetActive(false);
            }
            Color ribbon=Color.Lerp(DressColor,Color.white,.6f);
            art.Piece("蝴蝶结左边",new Vector3(-.1f,1.9f,.06f),new Vector3(.23f,.16f,.1f),ribbon,PrimitiveType.Sphere);
            art.Piece("蝴蝶结右边",new Vector3(.1f,1.9f,.06f),new Vector3(.23f,.16f,.1f),ribbon,PrimitiveType.Sphere);
            art.Piece("蝴蝶结中心",new Vector3(0,1.9f,.07f),Vector3.one*.1f,DressColor,PrimitiveType.Sphere);
            art.Piece("裙摆小花",new Vector3(.16f,.59f,.3f),Vector3.one*.09f,ribbon,PrimitiveType.Sphere);
            faces=new Transform[7];
            for(int i=0;i<faces.Length;i++)
            {
                var face=new GameObject(((CastleGirlMood)i).ToString()+"表情").transform;face.SetParent(transform,false);faces[i]=face;
                var a=face.gameObject.AddComponent<RoyalArt>();Color mouth=new Color(.58f,.2f,.26f);
                if(i==1)
                {
                    a.Piece("难过的小嘴",new Vector3(0,1.43f,.219f),new Vector3(.095f,.025f,.025f),mouth);
                    foreach(float x in new[]{-.1f,.1f}){a.Piece("眼泪",new Vector3(x,1.52f,.231f),new Vector3(.026f,.09f,.026f),new Color(.4f,.75f,.96f),PrimitiveType.Sphere);var brow=a.Piece("难过眉毛",new Vector3(x,1.7f,.2f),new Vector3(.09f,.018f,.02f),hair);brow.localRotation=Quaternion.Euler(0,0,x<0?22:-22);}
                }
                else if(i==2)
                {
                    a.Piece("生气的小嘴",new Vector3(0,1.44f,.219f),new Vector3(.11f,.025f,.025f),mouth);
                    foreach(float x in new[]{-.1f,.1f}){var brow=a.Piece("生气眉毛",new Vector3(x,1.68f,.225f),new Vector3(.1f,.028f,.025f),hair);brow.localRotation=Quaternion.Euler(0,0,x<0?-25:25);}
                }
                else if(i==3||i==5)
                {
                    a.Piece(i==3?"笑开的小嘴":"惊讶的小嘴",new Vector3(0,1.44f,.22f),new Vector3(i==3?.12f:.07f,i==3?.075f:.08f,.035f),mouth,PrimitiveType.Sphere);
                    if(i==3)a.Piece("笑容里的牙齿",new Vector3(0,1.465f,.24f),new Vector3(.08f,.021f,.012f),Color.white);
                    if(i==5)foreach(float x in new[]{-.1f,.1f})a.Piece("抬高的眉毛",new Vector3(x,1.73f,.22f),new Vector3(.08f,.02f,.02f),hair);
                }
                else
                {
                    a.Piece("小嘴中间",new Vector3(0,1.425f,.22f),new Vector3(i==4?.065f:.1f,.025f,.025f),mouth);
                    foreach(float x in new[]{-.045f,.045f})a.Piece("上扬嘴角",new Vector3(x,1.44f,.22f),new Vector3(.025f,.025f,.025f),mouth,PrimitiveType.Sphere);
                    if(i==4)foreach(float x in new[]{-.16f,.16f})a.Piece("害羞脸颊",new Vector3(x,1.52f,.215f),new Vector3(.07f,.035f,.018f),new Color(.96f,.56f,.61f),PrimitiveType.Sphere);
                    if(i==6)foreach(float x in new[]{-.1f,.1f})a.Piece("平静眼睫",new Vector3(x,1.6f,.247f),new Vector3(.08f,.018f,.01f),hair);
                }
            }
            SetMood(DefaultMood);
        }
        public void SetMood(CastleGirlMood mood){Mood=mood;if(faces!=null)for(int i=0;i<faces.Length;i++)faces[i].gameObject.SetActive(i==(int)mood);}
    }

    public class CastleBallPlay : MonoBehaviour
    {
        Sandbox game;Transform ball;RoyalArt[] girls;TextMesh[] bubbles;float age,playerPassRemaining,nextPass;int playerPasses;
        public static readonly Vector3 Court=new Vector3(31,15,5);
        public bool NearPlayer=>game!=null&&Vector3.Distance(game.Player.transform.position,Court)<5;
        public int Passes=>playerPasses;
        public void Initialize(Sandbox g)
        {
            game=g;var a=gameObject.AddComponent<RoyalArt>();var mat=a.Piece("小伙伴传球草垫",Court-Vector3.up*.04f,new Vector3(7,.04f,7),new Color(.74f,.65f,.88f),PrimitiveType.Cylinder,true);UseFlatDiscCollider(mat);
            girls=new RoyalArt[3];bubbles=new TextMesh[3];
            for(int i=0;i<3;i++)
            {
                girls[i]=CastleGirlStyle.Make(transform,4+i,game);float angle=i*Mathf.PI*2/3;girls[i].transform.position=Court+new Vector3(Mathf.Sin(angle)*2.6f,0,Mathf.Cos(angle)*2.6f);
                var label=new GameObject("传球对话").transform;label.SetParent(girls[i].transform,false);label.localPosition=Vector3.up*2.32f;
                var text=label.gameObject.AddComponent<TextMesh>();text.font=Resources.Load<Font>("NotoSansCJKsc-Regular");text.GetComponent<Renderer>().sharedMaterial=text.font.material;text.fontSize=36;text.characterSize=.035f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.4f,.19f,.38f);bubbles[i]=text;
            }
            ball=a.Piece("来回传递的彩色球",Court+Vector3.up,new Vector3(.36f,.36f,.36f),new Color(.94f,.45f,.67f),PrimitiveType.Sphere);
            var sign=new GameObject("G 加入小伙伴传球").transform;sign.SetParent(transform,false);sign.position=Court+new Vector3(0,1.6f,-3.7f);var signText=sign.gameObject.AddComponent<TextMesh>();signText.font=Resources.Load<Font>("NotoSansCJKsc-Regular");signText.GetComponent<Renderer>().sharedMaterial=signText.font.material;signText.text="G 加入小伙伴传球";signText.fontSize=36;signText.characterSize=.04f;signText.anchor=TextAnchor.MiddleCenter;signText.color=new Color(.35f,.2f,.4f);
        }
        public static void UseFlatDiscCollider(Transform disc)
        {
            // Unity's primitive cylinder uses a capsule collider. A very flat, wide
            // cylinder therefore still has a tall capsule: use the actual mesh here.
            var old=disc.GetComponent<Collider>();if(old!=null){old.enabled=false;Object.Destroy(old);}
            var surface=disc.gameObject.AddComponent<MeshCollider>();surface.sharedMesh=disc.GetComponent<MeshFilter>().sharedMesh;
        }
        public void PassFromPlayer()
        {
            if(game.Paused||!NearPlayer||Time.unscaledTime<nextPass)return;nextPass=Time.unscaledTime+.8f;playerPassRemaining=1.6f;playerPasses++;
            game.Notice="你把球传给了小伙伴，大家轮流玩，不抢着一直拿球。";
            game.StartPartyBallPlay();
            if(playerPasses>=3){playerPasses=0;game.EarnActivityCoins(18,"和城堡小伙伴轮流传球");}
        }
        void Update()
        {
            if(game==null||game.Paused)return;age+=Time.deltaTime;var players=new System.Collections.Generic.List<Transform>();if(game.PartyPlayingBall)players.AddRange(game.PartyMembers());foreach(var a in girls)if(a.gameObject.activeInHierarchy)players.Add(a.transform);ball.gameObject.SetActive(players.Count>0);if(players.Count==0)return;int turn=Mathf.FloorToInt(age/3)%players.Count,to=(turn+1)%players.Count;float t=(age%3)/3;
            Vector3 from=players[turn].position+Vector3.up*.95f,target=players[to].position+Vector3.up*.95f;
            if(playerPassRemaining>0){playerPassRemaining=Mathf.Max(0,playerPassRemaining-Time.deltaTime);float p=1-playerPassRemaining/1.6f;from=game.Player.transform.position+game.Player.transform.forward*.55f+Vector3.up;ball.position=Vector3.Lerp(from,target,p)+Vector3.up*Mathf.Sin(p*Mathf.PI)*.7f;}
            else ball.position=Vector3.Lerp(from,target,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.7f;
            for(int i=0;i<3;i++)
            {
                if(!girls[i].gameObject.activeInHierarchy)continue;
                Vector3 look=ball.position-girls[i].transform.position;look.y=0;if(look.sqrMagnitude>.01f)girls[i].transform.rotation=Quaternion.Slerp(girls[i].transform.rotation,Quaternion.LookRotation(look),Time.deltaTime*4);
                if(girls[i].Arm!=null)girls[i].Arm.localRotation=Quaternion.Euler(i==turn?-55:Mathf.Sin(age*2+i)*12,0,0);
                var style=girls[i].GetComponent<CastleGirlStyle>();style.SetMood(i==turn?CastleGirlMood.Angry:i==to?CastleGirlMood.Smile:CastleGirlMood.Tears);
                bubbles[i].text=i==turn?"轮到我啦！":i==to?"我们轮流接球。":"哎呀，刚才没接到。";
                Vector3 eye=game.Player.View.transform.position-bubbles[i].transform.position;if(eye.sqrMagnitude>.01f)bubbles[i].transform.rotation=Quaternion.LookRotation(-eye);
            }
        }
    }
}
