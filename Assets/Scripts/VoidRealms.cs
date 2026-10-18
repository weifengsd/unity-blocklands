using UnityEngine;
using System.Collections.Generic;

namespace Blocklands
{
    public partial class Sandbox
    {
        bool voidFallAnnounced, voidPlanetAnnounced, voidHellAnnounced;
        public readonly HashSet<int> LostSkyResidents = new HashSet<int>();
        public void RememberLostSkyResident(int id) { if (id >= 0 && id < 24) LostSkyResidents.Add(id); }
        public bool InVoidRealms => World != null && Player != null && Player.transform.position.y < RealmEntryY;
        public const float RealmEntryY=-550f, HeavenFloor=-650f, PlanetFloor=-870f, HellFloor=-1330f;
        public const float RealmDiameter=240f, PlanetDiameter=512f, PlanetLandDiameter=448f;
        public bool InHeaven => InVoidRealms && Player.transform.position.y > HeavenFloor-45;
        public bool InPlanetLife => InVoidRealms && Player.transform.position.y<=HeavenFloor-45 && Player.transform.position.y>HellFloor+120;
        public bool InHell => InVoidRealms && Player.transform.position.y <= HellFloor+120;
        public bool VoidRideActive => heavenSwingRemaining > 0 || RealmLifeLocked;
        Transform heavenSwing;
        float heavenSwingRemaining, nextHellHurt;bool heavenRideWasActive;

        void BuildVoidRealms(Transform parent)
        {
            if(VoidRideActive)Player.HomeRest(false);
            heavenSwingRemaining=0;heavenRideWasActive=false;realmMealRemaining=realmBathRemaining=0;realmResting=false;
            if(realmFood!=null){Destroy(realmFood.gameObject);realmFood=null;}
            var root = new GameObject("垂直奇境 · 上层天堂、星球、下层地狱").transform;
            root.SetParent(parent, false);
            var art = root.gameObject.AddComponent<RoyalArt>();
            var cloud = new Color(.88f, .91f, 1);
            var grass = new Color(.52f, .73f, .6f);
            var rock = new Color(.21f, .15f, .23f);
            void Slab(string name, Vector3 center, Vector3 size, Color color) => art.Piece(name, center, size, color, PrimitiveType.Cube, true);

            // Eight-by-six opening: the player must choose to walk into it to descend.
            // The first fall from the mined world lands on the upper platform, never auto-flies.
            BuildRealmGround(root,"天堂云岛可步行土地",HeavenFloor+1,120,grass,new Rect(44,29,8,6));
            foreach (var x in new[] { 44f, 52f })
                art.Piece("天堂向下入口护栏", new Vector3(x, HeavenFloor + 2, 32), new Vector3(.25f, 1.4f, 6), cloud);
            art.Piece("入口边缘金光", new Vector3(48, HeavenFloor + 1.12f, 28.6f), new Vector3(8, .12f, .35f), new Color(1, .86f, .52f));
            PlaceSign(root, "天空天堂 · 洞口向下通往有人居住的星球", new Vector3(32, HeavenFloor + 3, 27));

            // The round planet appears only after falling well below the heaven island.
            // It is a visual globe; editable terrain above remains voxel coordinates.
            BuildInhabitedPlanet(root,art);
            for (int i = 0; i < 13; i++)
            {
                float angle = i * 2 * Mathf.PI / 13;
                art.Piece("天堂云海", new Vector3(32 + Mathf.Cos(angle) * 114, HeavenFloor + 2, 32 + Mathf.Sin(angle) * 114),
                    new Vector3(19, 5, 12), cloud, PrimitiveType.Sphere);
            }
            BuildHeaven(root, art);

            // Hell sits below the large planet, far from the sky island.
            BuildRealmGround(root,"地狱岩台",HellFloor+1,80,rock,new Rect(19,45,6,6));
            Slab("地狱坑底", new Vector3(22, HellFloor - 19, 48), new Vector3(11, 2, 11), new Color(.12f, .08f, .12f));
            Slab("下层防止无限坠落的岩床", new Vector3(32, HellFloor - 48, 32), new Vector3(180, 2, 180), rock);
            BuildHell(root, art);
            BuildRealmLifeExpansion(root,art);
        }

        void PlaceSign(Transform parent, string message, Vector3 position)
        {
            var sign = new GameObject(message);sign.transform.SetParent(parent, false);
            sign.transform.localPosition = position;
            var text = sign.AddComponent<TextMesh>();text.font = Resources.Load<Font>("NotoSansCJKsc-Regular");
            text.text = message;text.fontSize = 48;text.characterSize = .032f;
            text.anchor = TextAnchor.MiddleCenter;text.alignment = TextAlignment.Center;
            text.color = new Color(1, .91f, .67f);text.GetComponent<Renderer>().sharedMaterial = text.font.material;
            sign.AddComponent<WorldTextDepth>();
            var board=sign.AddComponent<RoyalArt>();board.Piece("小型说明牌",new Vector3(0,0,.06f),new Vector3(Mathf.Clamp(message.Length*.16f,2,11),.7f,.06f),new Color(.21f,.18f,.25f));
        }

        RoyalArt RealmPerson(Transform parent, string title, Vector3 position, Color clothes, bool small, bool dress)
        {
            var root = new GameObject(title).transform;root.SetParent(parent, false);root.localPosition = position;
            if (small) root.localScale = Vector3.one * .78f;
            var a = root.gameObject.AddComponent<RoyalArt>();
            Color skin = new Color(.92f, .7f, .56f), hair = new Color(.24f, .15f, .16f);
            if (dress) a.Taper("简洁的衣裙", new Vector3(0, .57f, 0), new Vector3(.77f, .45f, .56f), clothes, .65f);
            else foreach (float x in new[] { -.16f, .16f })
                a.Piece("裤腿", new Vector3(x, .38f, 0), new Vector3(.24f, .71f, .28f), clothes);
            a.Piece("外衣", new Vector3(0, 1.03f, 0), new Vector3(.55f, .7f, .36f), clothes);
            a.Piece("脸", new Vector3(0, 1.67f, 0), new Vector3(.5f, .55f, .45f), skin, PrimitiveType.Sphere);
            a.Piece(dress ? "长头发" : "短头发", new Vector3(0, 1.91f, -.06f), new Vector3(.56f, .2f, .5f), hair, PrimitiveType.Sphere);
            foreach (float x in new[] { -.12f, .12f })
            {
                a.Piece("眼睛", new Vector3(x, 1.7f, .228f), Vector3.one * .06f, new Color(.18f, .12f, .17f), PrimitiveType.Sphere);
                a.Piece("手臂", new Vector3(x * 3.1f, 1.01f, 0), new Vector3(.17f, .58f, .2f), skin, PrimitiveType.Capsule);
                a.Piece("鞋子", new Vector3(x, .08f, .05f), new Vector3(.2f, .17f, .3f), clothes);
            }
            a.Piece("微笑", new Vector3(0, 1.54f, .23f), new Vector3(.14f, .03f, .02f), new Color(.64f, .3f, .34f));
            return a;
        }

        void BuildHeaven(Transform root, RoyalArt art)
        {
            float top = HeavenFloor + 1;
            var white = new Color(.96f, .95f, 1);
            var pink = new Color(.98f, .69f, .78f);
            var gold = new Color(1, .83f, .49f);
            var light = new GameObject("天堂柔光").AddComponent<Light>();light.transform.SetParent(root, false);
            light.transform.localPosition = new Vector3(31, top + 18, 31);light.type = LightType.Point;
            light.range = 90;light.intensity = .8f;light.color = new Color(1, .91f, .77f);
            foreach (var x in new[] { 20f, 32f })
            {
                BuildRealmHome(root,new Vector3(x,top,52),"天堂家园",white,pink);
            }
            var family = new[] {
                RealmPerson(root, "天堂的妈妈", new Vector3(21, top, 39), new Color(.82f, .52f, .71f), false, true),
                RealmPerson(root, "天堂的爸爸", new Vector3(24, top, 39), new Color(.54f, .69f, .84f), false, false),
                RealmPerson(root, "天堂的小孩", new Vector3(27, top, 39), new Color(.97f, .78f, .47f), true, false),
                RealmPerson(root, "天堂的小朋友", new Vector3(29, top, 40), new Color(.66f, .8f, .7f), true, true),
                RealmPerson(root, "会唱歌的天使", new Vector3(36, top, 20), new Color(.77f, .81f, .98f), false, true),
                RealmPerson(root, "照顾花园的天使", new Vector3(15, top, 18), new Color(.72f, .91f, .8f), false, false)
            };
            AddHaloAndWings(family[4], false);AddHaloAndWings(family[5], false);
            for (int i = 0; i < 12; i++)
            {
                float x = 8 + (i * 11) % 31, z = 10 + (i * 17) % 50;
                art.Piece("鲜花草丛", new Vector3(x, top + .28f, z), new Vector3(1.1f, .55f, 1.1f),
                    i % 2 == 0 ? pink : gold, PrimitiveType.Sphere);
            }
            // A real six-second ride, not just a decorative sign.
            art.Piece("云秋千横梁", new Vector3(12, top + 5, 44), new Vector3(5, .28f, .34f), gold);
            foreach (float x in new[] { 9.7f, 14.3f })
                art.Piece("云秋千支架", new Vector3(x, top + 2.7f, 44), new Vector3(.24f, 4.5f, .28f), white);
            heavenSwing = new GameObject("可以乘坐的云秋千").transform;
            heavenSwing.SetParent(root, false);heavenSwing.localPosition = new Vector3(12, top + 4.9f, 44);
            var swingArt = heavenSwing.gameObject.AddComponent<RoyalArt>();
            foreach (float x in new[] { -.9f, .9f })
                swingArt.Piece("云秋千绳索", new Vector3(x, -1.35f, 0), new Vector3(.08f, 2.7f, .08f), gold);
            swingArt.Piece("云秋千座椅", new Vector3(0, -2.7f, 0), new Vector3(2.2f, .24f, .9f), pink);
            var slide = art.Piece("可滑的云朵滑道", new Vector3(9, top + 1.2f, 28), new Vector3(2.4f, .45f, 9), white, PrimitiveType.Cube, true);
            slide.localRotation = Quaternion.Euler(-14, 0, 0);
            BuildRealmSpa(root,art);
            art.Piece("大家的野餐长桌", new Vector3(35, top + 1.2f, 40), new Vector3(7, .28f, 2.5f), gold);
            foreach (float x in new[] { 32.5f, 34f, 35.5f, 37f })
                AddRealmFood(art,new Vector3(x,top+1.55f,40),x<35?"天堂水果":"天堂面包",x<35?pink:new Color(.84f,.64f,.3f));
            PlaceSign(root, "G 野餐、云秋千、温泉", new Vector3(-2, top + 2.3f, 27));
        }

        void BuildHell(Transform root, RoyalArt art)
        {
            float top = HellFloor + 1;
            var red = new Color(.84f, .14f, .1f);
            var lava = new Color(.96f, .3f, .07f);
            var dark = new Color(.28f, .18f, .23f);
            var light = new GameObject("地狱火光").AddComponent<Light>();light.transform.SetParent(root, false);
            light.transform.localPosition = new Vector3(35, top + 9, 35);light.type = LightType.Point;
            light.range = 72;light.intensity = 2;light.color = lava;
            for (int i = 0; i < 15; i++)
            {
                float x = 7 + (i * 13) % 52, z = 7 + (i * 19) % 53;
                art.Piece("地狱熔岩裂纹", new Vector3(x, top + .06f, z), new Vector3(2.5f, .08f, .6f), lava);
                if (i % 3 == 0) art.Piece("赤色火柱", new Vector3(x, top + 1.1f, z), new Vector3(.55f, 2, .55f), lava, PrimitiveType.Capsule);
            }
            foreach (float x in new[] { 10f, 17f, 31f, 40f, 57f })
            {
                art.Piece("诡异岩柱", new Vector3(x, top + 3.1f, 15), new Vector3(2, 6, 2), dark);
                art.Piece("岩柱上的红色痕迹", new Vector3(x, top + 1, 13.9f), new Vector3(.32f, 2.3f, .08f), red);
            }
            art.Piece("深坑边缘警告", new Vector3(22, top + .12f, 44.5f), new Vector3(6, .12f, .35f), lava);
            PlaceSign(root, "地狱在星球下方 · 前面有深坑 · 掉入坑底按G爬出", new Vector3(28, top + 3.4f, 31));
            foreach (float x in new[] { 9f, 15f, 37f })
            {
                var ghost = new GameObject("徘徊的幽灵").transform;ghost.SetParent(root, false);
                ghost.localPosition = new Vector3(x, top + 2, 27 + x % 3 * 5);
                var shape = ghost.gameObject.AddComponent<RoyalArt>();
                shape.Piece("幽灵身影", Vector3.zero, new Vector3(1.2f, 2.3f, .75f), new Color(.78f, .77f, .85f), PrimitiveType.Capsule);
                foreach (float e in new[] { -.23f, .23f })
                    shape.Piece("幽灵的眼睛", new Vector3(e, .31f, .37f), Vector3.one * .16f, red, PrimitiveType.Sphere);
            }
            var dragon = new GameObject("低吼的赤色恶龙").transform;dragon.SetParent(root, false);
            dragon.localPosition = new Vector3(51, top + 2, 50);
            dragon.localRotation = Quaternion.Euler(0, 180, 0);
            dragon.localScale = Vector3.one * 1.15f;
            dragon.gameObject.AddComponent<DragonSculpt>().Build(
                new Color(.29f, .07f, .1f), new Color(.62f, .12f, .16f),
                new Color(.14f, .035f, .055f), new Color(.38f, .31f, .34f),
                new Color(1, .24f, .12f));
            var body = dragon.gameObject.AddComponent<RoyalArt>();
            for (int i = 0; i < 5; i++)
                body.Piece("恶龙鳞片上的红色液滴", new Vector3(-.65f + i * .3f, 3.7f - i * .25f, 4.3f),
                    Vector3.one * .15f, red, PrimitiveType.Sphere);
            art.Piece("恶龙脚边的赤红痕迹", new Vector3(50, top + .09f, 46), new Vector3(3.3f, .08f, 1.6f), red, PrimitiveType.Sphere);
        }

        void AddHaloAndWings(RoyalArt art, bool heroine)
        {
            var gold = new Color(1, .88f, .35f);
            art.Piece(heroine ? "皇冠上的光环" : "天使光环", new Vector3(0, 2.24f, 0), new Vector3(.62f, .06f, .62f), gold, PrimitiveType.Cylinder);
            foreach (float x in new[] { -1f, 1f })
            {
                var wing = art.Piece("天使翅膀", new Vector3(x * .52f, 1.25f, -.26f), new Vector3(.38f, .76f, .13f),
                    new Color(.98f, .98f, 1), PrimitiveType.Sphere);
                wing.localRotation = Quaternion.Euler(0, 0, x * -25);
            }
        }

        void AddHornsAndTrident(RoyalArt art)
        {
            var red = new Color(.84f, .12f, .16f);
            foreach (float x in new[] { -.17f, .17f })
                art.Piece("红色龙角", new Vector3(x, 2.05f, 0), new Vector3(.12f, .34f, .12f), red, PrimitiveType.Cylinder);
            art.Piece("红色三叉戟柄", new Vector3(.56f, 1.07f, .2f), new Vector3(.07f, 1.7f, .07f), red);
            foreach (float x in new[] { -.15f, 0f, .15f })
                art.Piece("红色叉尖", new Vector3(.56f + x, 1.96f, .2f), new Vector3(.06f, .35f, .06f), red);
            art.Piece("三叉戟上的红色水滴", new Vector3(.56f, 1.76f, .2f), Vector3.one * .08f, red, PrimitiveType.Sphere);
        }

        public bool TryVoidRealmG()
        {
            if (!InVoidRealms || Paused) return false;
            var p = Player.transform.position;
            if(TryRealmLifeG())return true;
            if (VoidRideActive) return true;
            if (InHeaven)
            {
                if (Vector3.Distance(p, new Vector3(12, HeavenFloor + 1, 44)) < 5)
                {
                    heavenSwingRemaining = 6;
                    Player.Teleport(heavenSwing.TransformPoint(new Vector3(0, -2.5f, 0)));
                    Player.HomeRest(true);Notice = "云秋千已经荡起来！六秒后回到草地。";
                }
                else if (Vector3.Distance(p, new Vector3(14, HeavenFloor + 1, 13)) < 5)
                {
                    StartRealmBath();
                }
                else if (Vector3.Distance(p, new Vector3(35, HeavenFloor + 1, 40)) < 5)
                {
                    StartRealmMeal("天堂小饼干");
                }
                else if (Vector3.Distance(p, new Vector3(24, HeavenFloor + 1, 39)) < 7)
                {
                    StartRealmMeal("爸爸妈妈递来的面包");
                }
                else Notice = "天堂在上层天空：有小屋、家人、天使、云秋千、滑道和野餐。向洞口走才会继续往下坠。";
            }
            else if (InHell)
            {
                if (p.y < HellFloor - 8)
                {
                    Player.Teleport(new Vector3(28, HellFloor + 1.15f, 48));
                    Notice = "你沿着深坑中的石阶爬回地狱岩台，没有被迫飞行。";
                }
                else if (Vector3.Distance(p, new Vector3(51, HellFloor + 2, 50)) < 9)
                    Notice = "赤色恶龙低吼；它的鳞片有红色液滴。地狱深处还有幽灵、岩浆裂纹和深坑。";
                else Notice = "地狱在星球下方，离天堂很远。注意前面的窟窿；掉下去按G可爬上来。";
            }
            else Notice = "这里是星球的生活大陆：房屋、居民、农田、森林和工作台都在地面上。东侧井口可继续坠入地狱。";
            return true;
        }

        void TickVoidRealm()
        {
            if (Paused) return;
            TickRealmLife();
            if (heavenSwing == null)
            {
                if (heavenRideWasActive) { heavenRideWasActive = false;Player.HomeRest(false); }
                return;
            }
            if (heavenSwingRemaining>0)
            {
                heavenRideWasActive = true;
                heavenSwingRemaining = Mathf.Max(0, heavenSwingRemaining - Time.unscaledDeltaTime);
                heavenSwing.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2.8f) * 25);
                var seat = heavenSwing.TransformPoint(new Vector3(0, -2.5f, 0));
                Player.Carry(seat - Player.transform.position);
            }
            else if (heavenRideWasActive)
            {
                heavenRideWasActive = false;
                heavenSwing.localRotation = Quaternion.identity;
                Player.HomeRest(false);
                Player.Teleport(new Vector3(12, HeavenFloor + 1.15f, 41));
                Notice = "云秋千玩好了，可以再去找家人或野餐。";
            }
            if (InHell && Time.time >= nextHellHurt)
            {
                Vector3 q = Player.transform.position;
                for (int i = 0; i < 15; i++)
                {
                    float x = 7 + (i * 13) % 52, z = 7 + (i * 19) % 53;
                    if (new Vector2(q.x - x, q.z - z).sqrMagnitude > 1.3f) continue;
                    if (Damage(.5f, "踩到地狱发亮的熔岩裂纹")) nextHellHurt = Time.time + 2;
                    break;
                }
            }
        }
    }
}
