using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    public enum RegionalEncounterKind { Photo, Parcel, Prism, Egg, Bamboo, Sluice, Salvage, Garden }

    // A visit owns these small, local stories. No terrain is edited and no quest spends inventory.
    // Integration: RefreshRegion -> BuildRegionalEncounters(villagePeople.transform).
    // Input: an unpaused G press -> TryRegionalEncounter(); G/H retain their existing meanings.
    public sealed class RegionalEncounter
    {
        public RegionalEncounterKind Kind;
        public string Title;
        public Vector3 Centre;
        public readonly List<Vector3> Nodes = new List<Vector3>();
        public readonly List<Transform> Props = new List<Transform>();
        public readonly List<Transform> Moving = new List<Transform>();
        public readonly int[] Values = new int[3];
        public readonly float[] Growth = new float[3];
        public Transform Root, Parcel;
        public TextMesh Caption;
        public int Progress;
        public bool Complete, Paid, Active;
        public float Clock;
        public float Tension => Mathf.PingPong(Clock * .7f, 1);
    }

    public sealed class RegionalEncounters : MonoBehaviour
    {
        Sandbox game;
        Font font;
        readonly List<TextMesh> labels = new List<TextMesh>();
        readonly List<RegionalEncounter> stations = new List<RegionalEncounter>();
        public IReadOnlyList<RegionalEncounter> Stations => stations;
        static readonly Color Wood = new Color(.47f, .28f, .15f);
        static readonly Color Gold = new Color(.97f, .73f, .28f);
        static readonly Color Mint = new Color(.25f, .89f, .72f);
        static readonly string[] LightNames = { "红", "绿", "蓝" };
        static readonly Color[] LightColors = { new Color(1, .25f, .26f), Mint, new Color(.34f, .62f, 1) };

        public RegionalEncounter Find(RegionalEncounterKind kind)
        {
            foreach (var s in stations) if (s.Kind == kind) return s;
            return null;
        }

        public RegionalEncounter FindByTitle(string title)
        {
            foreach (var s in stations) if (s.Title == title) return s;
            return null;
        }

        public void Initialize(Sandbox owner)
        {
            game = owner;
            font = Resources.Load<Font>("NotoSansCJKsc-Regular");
            switch (game.World.Region)
            {
                case 0: Add(RegionalEncounterKind.Photo, "森林摄影小亭", new Vector3(40, 19, 29)); break;
                case 1: Add(RegionalEncounterKind.Parcel, "风铃村暖心快递", new Vector3(24, 15, 29)); break;
                case 2: Add(RegionalEncounterKind.Prism, "水晶谷三色棱镜", new Vector3(35, 17, 17)); break;
                case 3: Add(RegionalEncounterKind.Egg, "梦影龙蛋守护台", new Vector3(22, 18, 20)); break;
                case 4: Add(RegionalEncounterKind.Bamboo, "竹谷风灯小桥", new Vector3(25, 17, 32)); break;
            }
            // These stations live at real negative world heights, not in replacement travel maps.
            Add(RegionalEncounterKind.Sluice, "熔岩观察渠", new Vector3(31, -49, 42));
            Add(RegionalEncounterKind.Photo, "深铁矿灯观景台", new Vector3(36, -75, 16));
            Add(RegionalEncounterKind.Prism, "星晶光路阵", new Vector3(40, -109, 31));
            Add(RegionalEncounterKind.Salvage, "星海打捞码头", new Vector3(42, -136, 25), true);
            Add(RegionalEncounterKind.Garden, "地心花园育苗站", new Vector3(39, -178, 36));
        }

        // Search around the requested altitude, including below zero. Reject hazards, trees,
        // cramped voxels instead of carving out player buildings or ore deposits.
        bool FindGround(Vector3 desired, out Vector3 result, bool floating = false, List<Vector3> occupied = null)
        {
            int baseY = Mathf.RoundToInt(desired.y);
            for (int radius = 0; radius <= 9; radius++)
                for (int dx = -radius; dx <= radius; dx++)
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        if (radius > 0 && Mathf.Abs(dx) != radius && Mathf.Abs(dz) != radius) continue;
                        int x = Mathf.FloorToInt(desired.x) + dx, z = Mathf.FloorToInt(desired.z) + dz;
                        if (x < 3 || x > 59 || z < 3 || z > 59) continue;
                        for (int offset = 0; offset <= (floating ? 0 : 12); offset++)
                            for (int sign = 0; sign < (offset == 0 ? 1 : 2); sign++)
                            {
                                int y = baseY + (sign == 0 ? -offset : offset);
                                var feet = new Vector3Int(x, y, z);
                                if (y < VoxelWorld.MinY + 2 || y > VoxelWorld.Height - 5) continue;
                                Block floor = game.World.Get(feet + Vector3Int.down);
                                if (!floating && (!Blocks.Solid(floor) || floor == Block.Lava || floor == Block.Wood || floor == Block.Leaves || floor == Block.Bamboo)) continue;
                                bool clear = true;
                                for (int px = -1; px <= 1 && clear; px++)
                                    for (int pz = -1; pz <= 1 && clear; pz++)
                                        for (int h = 0; h < 3; h++)
                                        {
                                            Block b = game.World.Get(feet + new Vector3Int(px, h, pz));
                                            if (b != Block.Air && (!floating || b != Block.Water)) { clear = false; break; }
                                        }
                                Vector3 candidate = new Vector3(x + .5f, y, z + .5f);
                                if (clear && occupied != null) foreach (var p in occupied) if (Vector3.Distance(p, candidate) < 2.5f) { clear = false; break; }
                                if (clear) { result = candidate; return true; }
                            }
                    }
            result = desired;
            return false;
        }

        void Add(RegionalEncounterKind kind, string title, Vector3 desired, bool floating = false)
        {
            if (!FindGround(desired, out var centre, floating)) return;
            var root = new GameObject(title).transform;
            root.SetParent(transform, false);
            root.position = centre;
            var s = new RegionalEncounter { Kind = kind, Title = title, Centre = centre, Root = root };
            var a = root.gameObject.AddComponent<RoyalArt>();
            stations.Add(s);
            switch (kind)
            {
                case RegionalEncounterKind.Photo: BuildPhoto(s, a); break;
                case RegionalEncounterKind.Parcel: BuildParcels(s, a); break;
                case RegionalEncounterKind.Prism: BuildPrisms(s, a); break;
                case RegionalEncounterKind.Egg: BuildEgg(s, a); break;
                case RegionalEncounterKind.Bamboo: BuildBridge(s, a); break;
                case RegionalEncounterKind.Sluice: BuildSluice(s, a); break;
                case RegionalEncounterKind.Salvage: BuildSalvage(s, a); break;
                case RegionalEncounterKind.Garden: BuildGarden(s, a); break;
            }
            // A heavily rebuilt area can have no safe space. Do not expose overlapping,
            // unreachable controls or erase the player's construction to make room.
            for (int i = 0; i < s.Nodes.Count; i++) for (int j = 0; j < i; j++)
                if (Vector3.Distance(s.Nodes[i], s.Nodes[j]) < 2.5f)
                {
                    stations.Remove(s); root.gameObject.SetActive(false); Destroy(root.gameObject); return;
                }
            Refresh(s);
        }

        TextMesh Label(Transform parent, string text, Vector3 local, float scale = .065f)
        {
            var t = new GameObject("地区互动提示").AddComponent<TextMesh>();
            t.transform.SetParent(parent, false);
            t.transform.localPosition = local;
            t.text = text; t.fontSize = 32; t.characterSize = scale;
            t.anchor = TextAnchor.MiddleCenter; t.color = Color.white;
            if (font != null) { t.font = font; t.GetComponent<Renderer>().sharedMaterial = font.material; }
            labels.Add(t);
            return t;
        }

        void Board(RegionalEncounter s, RoyalArt a, string clue, Vector3 local)
        {
            a.Piece("说明牌柱", local + Vector3.up, new Vector3(.12f, 2, .12f), Wood);
            a.Piece("说明牌", local + Vector3.up * 2, new Vector3(3.1f, 1.1f, .12f), new Color(.12f, .24f, .3f));
            s.Caption = Label(s.Root, s.Title + "\n" + clue + "\nG 互动 · 免费", local + new Vector3(0, 2, -.1f), .065f);
        }

        Vector3 Node(RegionalEncounter s, Vector3 desiredLocal, bool fixedHeight = false)
        {
            Vector3 desired = s.Centre + desiredLocal, position;
            if (fixedHeight) position = desired;
            else if (!FindGround(desired, out position, false, s.Nodes)) position = s.Centre;
            s.Nodes.Add(position);
            return position - s.Centre;
        }

        void BuildPhoto(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, s.Centre.y < 0 ? "矿灯投影：电脑调至星夜，再到矿洞相框留影" : "任务：把电脑调至星夜，再去相框留影", new Vector3(-1.8f, 0, .8f));
            var p = Node(s, Vector3.zero);
            a.Piece("摄影电脑桌", p + new Vector3(0, .8f, .3f), new Vector3(1.7f, .16f, .85f), Wood);
            foreach (float x in new[] { -.65f, .65f }) a.Piece("桌腿", p + new Vector3(x, .4f, .3f), new Vector3(.12f, .8f, .12f), Wood);
            a.Piece("显示器边框", p + new Vector3(0, 1.42f, .45f), new Vector3(1.3f, .86f, .12f), new Color(.09f, .12f, .17f));
            s.Props.Add(a.Piece("可切换摄影电脑屏幕", p + new Vector3(0, 1.43f, .37f), new Vector3(1.15f, .7f, .035f), Gold));
            a.Piece("键盘", p + new Vector3(0, .91f, -.01f), new Vector3(.9f, .05f, .32f), new Color(.8f, .84f, .86f));
            a.Piece("电脑支座", p + new Vector3(0, 1.03f, .45f), new Vector3(.2f, .35f, .13f), Color.gray);
            foreach (float x in new[] { -1.35f, 1.35f }) a.Piece("摄影小亭木柱", p + new Vector3(x, 1.65f, .85f), new Vector3(.13f, 3.3f, .13f), Wood);
            a.Piece("摄影小亭遮阳顶", p + new Vector3(0, 3.3f, .35f), new Vector3(3.2f, .18f, 2.1f), new Color(.48f, .7f, .54f));
            Label(s.Root, "G 切换晨光 / 晴昼 / 星夜", p + new Vector3(0, 2.45f, 0));
            var frame = Node(s, new Vector3(0, 0, -5));
            a.Piece("留影地毯", frame + new Vector3(0, .02f, 0), new Vector3(2, .04f, 1.5f), new Color(.45f, .7f, .52f));
            foreach (float x in new[] { -1.3f, 1.3f }) a.Piece("相框侧边", frame + new Vector3(x, 1.4f, .6f), new Vector3(.16f, 2.8f, .15f), Gold);
            a.Piece("相框上边", frame + new Vector3(0, 2.8f, .6f), new Vector3(2.75f, .18f, .15f), Gold);
            a.Piece("三脚架相机", frame + new Vector3(1.8f, 1.2f, -1), new Vector3(.5f, .35f, .3f), new Color(.11f, .14f, .19f));
            a.Piece("相机镜头", frame + new Vector3(1.8f, 1.2f, -.8f), new Vector3(.22f, .1f, .22f), Mint, PrimitiveType.Cylinder).localRotation = Quaternion.Euler(90, 0, 0);
            a.Piece("三脚架", frame + new Vector3(1.8f, .55f, -1), new Vector3(.12f, 1.1f, .12f), Color.gray);
            Label(s.Root, "G 星夜纪念照（游戏内留影）", frame + Vector3.up * 3.2f);
            for (int i = 0; i < 5; i++) s.Moving.Add(a.Piece("相框星光", frame + new Vector3(-1 + i * .5f, 2.55f, .58f), Vector3.one * .12f, Color.white, PrimitiveType.Sphere));
        }

        void BuildParcels(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "邮差取件 → 面包师 → 花匠；不收邮费", new Vector3(-1.9f, 0, -1));
            var route = new[] { Vector3.zero, new Vector3(10, 0, 0), new Vector3(6, 0, 14) };
            var names = new[] { "邮差小风 · G 取两份包裹", "面包师 · G 送烤箱零件", "花匠 · G 送花种" };
            for (int i = 0; i < route.Length; i++)
            {
                var p = Node(s, route[i]);
                var person = RoyalArt.Girl(s.Root, false, false);
                person.name = names[i]; person.transform.localPosition = p + Vector3.forward * .6f;
                Label(s.Root, names[i], p + Vector3.up * 2.5f);
                a.Piece("快递木箱", p + new Vector3(.9f, .38f, .5f), new Vector3(.65f, .76f, .65f), Wood);
                a.Piece("包裹封条", p + new Vector3(.9f, .78f, .5f), new Vector3(.18f, .035f, .7f), Gold);
            }
            s.Parcel = a.Piece("手中待送包裹", new Vector3(0, .6f, -.4f), new Vector3(.45f, .35f, .4f), Gold);
            s.Parcel.gameObject.SetActive(false);
        }

        void BuildPrisms(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "目标：左绿 / 中蓝 / 右绿；G 转棱镜", new Vector3(0, 0, -2));
            for (int i = 0; i < 3; i++)
            {
                var p = Node(s, new Vector3((i - 1) * 3.5f, 0, 1));
                a.Piece("棱镜底座", p + Vector3.up * .45f, new Vector3(.9f, .9f, .9f), new Color(.23f, .28f, .36f));
                var crystal = a.Piece("可旋转三色棱镜", p + Vector3.up * 1.35f, new Vector3(.65f, 1, .65f), LightColors[0]);
                crystal.localRotation = Quaternion.Euler(0, 45, 25); s.Props.Add(crystal);
                s.Moving.Add(a.Piece("彩色光束", p + new Vector3(0, 1.35f, 1.6f), new Vector3(.09f, .09f, 2.5f), LightColors[0]));
                a.Piece("光束接收板", p + new Vector3(0, 1.3f, 3), new Vector3(.9f, 1.25f, .15f), new Color(.89f, .92f, .93f));
                Label(s.Root, (i == 0 ? "左：绿" : i == 1 ? "中：蓝" : "右：绿") + "\nG 转动", p + Vector3.up * 2.6f);
            }
            Node(s, new Vector3(0, 0, -3));
        }

        void BuildEgg(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "守护顺序：月亮 → 太阳 → 叶片 → 龙蛋", new Vector3(0, 0, -2));
            var symbols = new[] { "太阳", "叶片", "月亮" };
            for (int i = 0; i < 3; i++)
            {
                var p = Node(s, new Vector3((i - 1) * 3.5f, 0, 1));
                a.Piece("守护石", p + Vector3.up * .6f, new Vector3(.8f, 1.2f, .8f), new Color(.25f, .19f, .37f));
                s.Props.Add(a.Piece("守护印记", p + Vector3.up * 1.4f, Vector3.one * .5f, i == 0 ? Gold : i == 1 ? Mint : LightColors[2], PrimitiveType.Sphere));
                Label(s.Root, symbols[i] + " · G", p + Vector3.up * 2.2f);
            }
            var egg = Node(s, new Vector3(0, 0, 5));
            a.Piece("龙蛋软巢", egg + Vector3.up * .16f, new Vector3(1.6f, .16f, 1.6f), Wood, PrimitiveType.Cylinder);
            s.Moving.Add(a.Piece("被守护的水晶龙蛋", egg + Vector3.up * .9f, new Vector3(.85f, 1.35f, .85f), new Color(.82f, .62f, .99f), PrimitiveType.Sphere));
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3;
                s.Moving.Add(a.Piece("可解除守护光栏", egg + new Vector3(Mathf.Cos(angle), 1, Mathf.Sin(angle)), new Vector3(.06f, 2, .06f), LightColors[2]));
            }
            Label(s.Root, "水晶龙蛋 · G 安抚救援\n不影响梦影龙战斗", egg + Vector3.up * 2.7f);
        }

        void BuildBridge(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "G 放下竹桥 / 点亮风灯；再按可收起", new Vector3(-1.6f, 0, -1));
            Node(s, Vector3.zero);
            a.Piece("竹桥摇柄支柱", new Vector3(.7f, .65f, 0), new Vector3(.25f, 1.3f, .25f), Wood);
            var handle = a.Piece("竹桥摇柄", new Vector3(.7f, 1.4f, 0), new Vector3(.8f, .1f, .1f), Gold); s.Props.Add(handle);
            // A decorative bridge over intact safe ground: switching it off cannot drop a player in lava.
            for (int i = 0; i < 8; i++)
            {
                var plank = a.Piece("可展开竹桥板", new Vector3(0, .07f, 1.3f + i * .48f), new Vector3(1.9f, .14f, .42f), new Color(.65f, .73f, .31f));
                s.Moving.Add(plank);
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = new Vector3(i % 2 == 0 ? -1.3f : 1.3f, 0, 1.3f + i * 1.3f);
                a.Piece("风灯竹竿", p + Vector3.up * 1.2f, new Vector3(.1f, 2.4f, .1f), new Color(.38f, .52f, .24f));
                var lantern = a.Piece("可点亮风灯", p + Vector3.up * 2, new Vector3(.48f, .65f, .48f), Gold, PrimitiveType.Sphere); s.Props.Add(lantern);
                a.Piece("灯笼流苏", p + Vector3.up * 1.53f, new Vector3(.05f, .3f, .05f), Gold);
            }
        }

        void BuildSluice(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "G 开关熔岩闸门；观察渠内流动", new Vector3(-1.3f, 0, 0));
            Node(s, Vector3.zero, true);
            Color stone = new Color(.2f, .25f, .3f);
            a.Piece("封闭熔岩渠底", new Vector3(1.15f, .13f, 3.4f), new Vector3(1.15f, .25f, 5.8f), stone);
            foreach (float x in new[] { .55f, 1.75f }) a.Piece("熔岩渠护墙", new Vector3(x, .6f, 3.4f), new Vector3(.17f, .95f, 6), stone);
            foreach (float z in new[] { .45f, 6.35f }) a.Piece("熔岩渠端墙", new Vector3(1.15f, .6f, z), new Vector3(1.3f, .95f, .15f), stone);
            for (int i = 0; i < 20; i++) a.Piece("熔岩渠安全格栅", new Vector3(1.15f, 1.1f, .6f + i * .29f), new Vector3(1.25f, .06f, .07f), new Color(.47f, .56f, .6f));
            s.Props.Add(a.Piece("GATE升降闸板", new Vector3(1.15f, .67f, 1), new Vector3(1, .9f, .16f), new Color(.48f, .58f, .63f)));
            s.Props.Add(a.Piece("连续熔岩液面", new Vector3(1.15f, .3f, 3.4f), new Vector3(.96f, .08f, 5.5f), new Color(.95f, .16f, .015f)));
            for (int i = 0; i < 10; i++) s.Moving.Add(a.Piece("安全观察熔岩流", new Vector3(1.15f, .36f, .75f + i * .5f), new Vector3(.95f, .12f, .42f), i % 2 == 0 ? new Color(1, .32f, .04f) : Gold));
            a.Piece("GATE手轮", new Vector3(0, 1.05f, .6f), new Vector3(.6f, .12f, .6f), Gold, PrimitiveType.Cylinder).localRotation = Quaternion.Euler(90, 0, 0);
            Label(s.Root, "熔岩闸门 · G\n站在护栏外，看看流动的熔岩", new Vector3(0, 2.3f, .5f));
        }

        void BuildSalvage(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "G 开始绞盘；指针进绿色带再按 G", new Vector3(-1.8f, 0, 1));
            Node(s, Vector3.zero, true);
            a.Piece("星海打捞浮台", new Vector3(0, -.16f, .3f), new Vector3(3.8f, .3f, 4.2f), Wood, PrimitiveType.Cube, true);
            a.Piece("打捞架立柱", new Vector3(1.25f, 1.8f, 1.7f), new Vector3(.2f, 3.6f, .2f), Wood);
            a.Piece("打捞架横梁", new Vector3(1.25f, 3.5f, 2.6f), new Vector3(.2f, .2f, 2), Wood);
            s.Props.Add(a.Piece("打捞卷轴", new Vector3(.65f, 1.15f, .4f), new Vector3(.7f, .2f, .7f), Gold, PrimitiveType.Cylinder));
            a.Piece("救援绳", new Vector3(1.25f, .8f, 3.4f), new Vector3(.035f, 5.4f, .035f), Gold);
            s.Moving.Add(a.Piece("待打捞的密封补给箱", new Vector3(1.25f, -2, 3.4f), new Vector3(.85f, .65f, .7f), new Color(.3f, .72f, .77f)));
            for (int i = 0; i < 3; i++)
            {
                var crate = a.Piece("已救回的星海补给箱", new Vector3(-1 + i * .7f, .3f, -.6f), new Vector3(.55f, .55f, .55f), Gold);
                s.Moving.Add(crate);
            }
            Label(s.Root, "三箱补给 · 绿色区间收绳\n失误无损失，可继续", new Vector3(0, 2.1f, -.6f));
        }

        void BuildGarden(RegionalEncounter s, RoyalArt a)
        {
            Board(s, a, "G 播种 → 浇水 → 等待开花 → 采收", new Vector3(0, 0, -2));
            for (int i = 0; i < 3; i++)
            {
                var p = Node(s, new Vector3((i - 1) * 3.3f, 0, 1));
                a.Taper("育苗陶盆", p + Vector3.up * .3f, new Vector3(1.2f, .3f, 1.2f), new Color(.65f, .32f, .24f), 1.3f);
                a.Piece("花盆土壤", p + Vector3.up * .59f, new Vector3(1.04f, .06f, 1.04f), new Color(.22f, .15f, .12f), PrimitiveType.Cylinder);
                var plant = new GameObject("会生长的星光花").transform; plant.SetParent(s.Root, false); plant.localPosition = p + Vector3.up * .61f;
                var flower = plant.gameObject.AddComponent<RoyalArt>();
                flower.Piece("花茎", new Vector3(0, .48f, 0), new Vector3(.09f, .96f, .09f), Mint);
                foreach (float side in new[] { -1f, 1f }) flower.Piece("叶片", new Vector3(side * .18f, .3f, 0), new Vector3(.4f, .12f, .22f), Mint, PrimitiveType.Sphere).localRotation = Quaternion.Euler(0, 0, side * 25);
                for (int petal = 0; petal < 5; petal++)
                {
                    float angle = petal * Mathf.PI * .4f;
                    flower.Piece("花瓣", new Vector3(Mathf.Cos(angle) * .25f, 1, Mathf.Sin(angle) * .25f), new Vector3(.45f, .15f, .45f), Color.HSVToRGB(.78f + i * .065f, .4f, 1), PrimitiveType.Sphere);
                }
                flower.Piece("花心", Vector3.up * 1.05f, Vector3.one * .23f, Gold, PrimitiveType.Sphere);
                s.Props.Add(plant);
                Label(s.Root, "苗圃 " + (i + 1) + " · G\n免费种子与泉水", p + Vector3.up * 2.2f);
            }
            Node(s, new Vector3(0, 0, -3));
            a.Piece("浇水壶", new Vector3(2.8f, .45f, -1.5f), new Vector3(.6f, .75f, .5f), new Color(.3f, .69f, .81f), PrimitiveType.Cylinder);
            a.Piece("浇水壶壶嘴", new Vector3(3.2f, .55f, -1.5f), new Vector3(.65f, .13f, .13f), new Color(.3f, .69f, .81f)).localRotation = Quaternion.Euler(0, 0, 25);
        }

        public bool Interact()
        {
            if (game == null || !game.FestivalInputAvailable || game.FestivalLocked || game.Player == null) return false;
            if (!Nearest(game.Player.transform.position, 2.7f, out var s, out int node)) return false;
            return Use(s, node);
        }

        bool Nearest(Vector3 position, float range, out RegionalEncounter result, out int index)
        {
            result = null; index = -1;
            foreach (var s in stations) for (int i = 0; i < s.Nodes.Count; i++)
            {
                float d = Vector3.Distance(position, s.Nodes[i]);
                if (d < range) { range = d; result = s; index = i; }
            }
            return result != null;
        }

        bool Use(RegionalEncounter s, int node)
        {
            switch (s.Kind)
            {
                case RegionalEncounterKind.Photo:
                    if (node == 0) { s.Values[0] = (s.Values[0] + 1) % 3; s.Complete=false;game.Notice = "电脑摄影场景：" + PhotoMode(s.Values[0]) + "。去金色相框按 G 留影。"; }
                    else if (s.Values[0] == 2&&!s.Complete) Finish(s, s.Centre.y < 0 ? "星夜矿灯纪念照已留在观景台！电脑可换投影继续玩。" : "星夜纪念照已留在小亭！电脑可换景继续玩。");
                    else if(s.Complete)game.Notice="这张照片已经拍好，去电脑换景再拍下一张。";
                    else game.Notice = "照片场景还不是星夜。先去电脑按 G 调到星夜。";
                    break;
                case RegionalEncounterKind.Parcel:
                    if (node == 0 && (s.Progress == 0 || s.Complete)) { s.Progress = 1; s.Complete = false; game.Notice = "领到两份包裹：先交给面包师，再给花匠。无需付费。"; }
                    else if (node == 1 && s.Progress == 1) { s.Progress = 2; game.Notice = "面包师收到零件！最后一份花种送到花匠处。"; }
                    else if (node == 2 && s.Progress == 2) { s.Progress = 3; Finish(s, "两份包裹都送到了，村民向你道谢！"); }
                    else game.Notice = s.Complete ? "配送已完成；回邮差处 G 可免费再跑一趟。" : "配送顺序：邮差取件 → 面包师 → 花匠。";
                    break;
                case RegionalEncounterKind.Prism:
                    if (node == 3) { for (int i = 0; i < 3; i++) s.Values[i] = 0; s.Complete = false; game.Notice = "棱镜已复位。目标：绿、蓝、绿。"; }
                    else { s.Values[node] = (s.Values[node] + 1) % 3; s.Complete = false; game.Notice = "光路：" + PrismState(s) + "；目标：绿 / 蓝 / 绿"; if (s.Values[0] == 1 && s.Values[1] == 2 && s.Values[2] == 1) Finish(s, s.Centre.y < 0 ? "星晶洞窟的三色光路已接通！" : "三色光路校准成功，水晶谷亮起来了！"); }
                    break;
                case RegionalEncounterKind.Egg:
                    if (s.Complete) { game.Notice = "龙蛋已获救；守护任务本次来访完成。梦影龙仍在原处。"; break; }
                    if (node == 3) { if (s.Progress == 3) Finish(s, "光栏解除！你安抚了水晶龙蛋，让它安心留在软巢里。"); else game.Notice = "先依次激活月亮、太阳、叶片，再来安抚龙蛋。"; }
                    else if (s.Progress < 3 && node == new[] { 2, 0, 1 }[s.Progress]) { s.Progress++; game.Notice = "守护印记 " + s.Progress + " / 3" + (s.Progress == 3 ? "：去软巢按 G 安抚龙蛋。" : "。继续按说明牌顺序。"); }
                    else { s.Progress = 0; game.Notice = "顺序不符，印记温柔地复位。再试：月亮 → 太阳 → 叶片。"; }
                    break;
                case RegionalEncounterKind.Bamboo:
                case RegionalEncounterKind.Sluice:
                    s.Active = !s.Active;
                    if (s.Active) Finish(s, s.Kind == RegionalEncounterKind.Bamboo ? "竹桥展开，三盏风灯点亮！再按 G 可收起。" : "熔岩闸门 已打开：熔岩在封闭观察渠内流动。G 可关闭。");
                    else game.Notice = s.Kind == RegionalEncounterKind.Bamboo ? "竹桥收起、风灯熄灭；下方原有安全地面不变。" : "熔岩闸门 已关闭，观察渠流动停止；没有改变真实地形。";
                    break;
                case RegionalEncounterKind.Salvage:
                    if (s.Complete) { s.Progress = 0; s.Complete = false; s.Active = false; game.Notice = "绞盘免费复位。再按 G 开始；再次完成也能领到金币。"; }
                    else if (!s.Active) { s.Active = true; s.Clock = 0; game.Notice = "绞盘启动！等指针进入绿色区域，再按 G 收绳。"; }
                    else if (s.Tension >= .35f && s.Tension <= .65f) { s.Active = false; s.Progress++; if (s.Progress == 3) Finish(s, "三箱星海补给都打捞成功！"); else game.Notice = "打捞成功 " + s.Progress + " / 3。G 开始下一箱。"; }
                    else { s.Active = false; game.Notice = "绳索松开了，没有损失。G 重试这一箱，已救回的箱子保留。"; }
                    break;
                case RegionalEncounterKind.Garden:
                    if (node == 3) { if (!s.Complete) { game.Notice = "先照料并采收三盆花；完成后可在此免费重新播种。"; break; } for (int i = 0; i < 3; i++) { s.Values[i] = 0; s.Growth[i] = 0; } s.Progress = 0; s.Complete = false; game.Notice = "三只花盆已经整理好，可再种一轮。"; }
                    else if (s.Values[node] == 0) { s.Values[node] = 1; game.Notice = "已播下星光花种。再按 G 浇水。"; }
                    else if (s.Values[node] == 1) { s.Values[node] = 2; s.Growth[node] = 0; game.Notice = "泉水浇好了；约 4 秒后开花，开花后 G 采收。"; }
                    else if (s.Values[node] == 2) game.Notice = "小苗正在生长，剩余 " + Mathf.CeilToInt(4 - s.Growth[node]) + " 秒。";
                    else if (s.Values[node] == 3) { s.Values[node] = 4; s.Progress++; if (s.Progress == 3) Finish(s, "三盆星光花采收完成！免费种植，不消耗背包物品。"); else game.Notice = "采收星光花 " + s.Progress + " / 3。继续照料另外两盆。"; }
                    else game.Notice = "这盆已采收；三盆全部完成后，到说明牌前 G 重种。";
                    break;
            }
            Refresh(s);
            return true;
        }

        void Finish(RegionalEncounter s, string text)
        {
            s.Complete = true;
            s.Paid = true;
            int earned=game.EarnActivityCoins(12,s.Title);
            if(earned>0)game.Notice=text+" 获得"+earned+"元；下次完成也有奖励。";
        }

        static string PhotoMode(int n) => n == 0 ? "晨光" : n == 1 ? "晴昼" : "星夜";
        static string PrismState(RegionalEncounter s) => LightNames[s.Values[0]] + " / " + LightNames[s.Values[1]] + " / " + LightNames[s.Values[2]];
        static void Tint(Transform t, Color color) { var r = t.GetComponent<Renderer>(); if (r != null) r.sharedMaterial.color = color; }

        void Refresh(RegionalEncounter s)
        {
            switch (s.Kind)
            {
                case RegionalEncounterKind.Photo:
                    Tint(s.Props[0], s.Values[0] == 0 ? Gold : s.Values[0] == 1 ? Mint : new Color(.27f, .27f, .62f));
                    foreach (var star in s.Moving) star.gameObject.SetActive(s.Values[0] == 2);
                    break;
                case RegionalEncounterKind.Parcel: s.Parcel.gameObject.SetActive(s.Progress > 0 && s.Progress < 3); break;
                case RegionalEncounterKind.Prism:
                    for (int i = 0; i < 3; i++) { Tint(s.Props[i], LightColors[s.Values[i]]); Tint(s.Moving[i], LightColors[s.Values[i]]); s.Props[i].localRotation = Quaternion.Euler(0, 45 + s.Values[i] * 120, 25); }
                    break;
                case RegionalEncounterKind.Egg:
                    for (int i = 0; i < 3; i++) { bool lit = i == 2 && s.Progress > 0 || i == 0 && s.Progress > 1 || i == 1 && s.Progress > 2; s.Props[i].localScale = Vector3.one * (lit ? .68f : .5f); }
                    for (int i = 1; i < s.Moving.Count; i++) s.Moving[i].gameObject.SetActive(!s.Complete);
                    if (s.Complete) Tint(s.Moving[0], Gold);
                    break;
                case RegionalEncounterKind.Bamboo:
                    s.Props[0].localRotation = Quaternion.Euler(0, 0, s.Active ? 45 : -45);
                    for (int i = 1; i < s.Props.Count; i++) Tint(s.Props[i], s.Active ? Gold : new Color(.26f, .3f, .2f));
                    foreach (var plank in s.Moving) plank.gameObject.SetActive(s.Active);
                    break;
                case RegionalEncounterKind.Sluice:
                    s.Props[0].localPosition = new Vector3(1.15f, s.Active ? 1.5f : .67f, 1);
                    s.Props[1].gameObject.SetActive(s.Active);
                    foreach (var flow in s.Moving) flow.gameObject.SetActive(s.Active);
                    break;
                case RegionalEncounterKind.Salvage:
                    for (int i = 1; i < s.Moving.Count; i++) s.Moving[i].gameObject.SetActive(i <= s.Progress);
                    s.Moving[0].gameObject.SetActive(!s.Complete);
                    break;
                case RegionalEncounterKind.Garden:
                    for (int i = 0; i < 3; i++)
                    {
                        s.Props[i].gameObject.SetActive(s.Values[i] >= 1 && s.Values[i] < 4);
                        s.Props[i].localScale = Vector3.one * (s.Values[i] >= 3 ? 1 : .18f + s.Growth[i] * .18f);
                        foreach (Transform part in s.Props[i]) if (part.name == "花瓣" || part.name == "花心") part.gameObject.SetActive(s.Values[i] >= 3);
                    }
                    break;
            }
        }

        public void Tick(float dt)
        {
            if (game == null || game.Paused) return;
            foreach (var s in stations)
            {
                if (s.Active) s.Clock += dt;
                if (s.Kind == RegionalEncounterKind.Parcel && s.Parcel.gameObject.activeSelf)
                {
                    s.Parcel.position = game.Player.transform.position + game.Player.transform.forward * .75f + game.Player.transform.right * .46f + Vector3.up * .85f;
                    s.Parcel.rotation = game.Player.transform.rotation;
                }
                if (s.Kind == RegionalEncounterKind.Sluice && s.Active)
                    for (int i = 0; i < s.Moving.Count; i++) s.Moving[i].localPosition = new Vector3(1.15f, .36f + Mathf.Sin(s.Clock * 5 + i) * .015f, .8f + Mathf.Repeat(i * .5f + s.Clock * 1.8f, 5.2f));
                if (s.Kind == RegionalEncounterKind.Salvage && s.Active)
                {
                    s.Props[0].Rotate(0, dt * 150, 0);
                    s.Moving[0].localPosition = new Vector3(1.25f, Mathf.Lerp(-2, .1f, s.Tension), 3.4f);
                }
                if (s.Kind == RegionalEncounterKind.Garden)
                {
                    bool changed = false;
                    for (int i = 0; i < 3; i++) if (s.Values[i] == 2) { s.Growth[i] = Mathf.Min(4, s.Growth[i] + dt); if (s.Growth[i] >= 4) s.Values[i] = 3; changed = true; }
                    if (changed) Refresh(s);
                }
            }
        }

        void Update() { if (game != null) Tick(Time.deltaTime); }
        void LateUpdate()
        {
            if (game == null || game.Player == null) return;
            foreach (var label in labels) if (label != null) label.transform.rotation = game.Player.View.transform.rotation;
        }

        public string Hint(RegionalEncounter s)
        {
            switch (s.Kind)
            {
                case RegionalEncounterKind.Photo: return "电脑：" + PhotoMode(s.Values[0]) + " · 调到星夜后去金色相框 G 留影";
                case RegionalEncounterKind.Parcel: return s.Progress == 0 ? "G 邮差取件；再送给面包师和花匠" : s.Progress == 1 ? "包裹 0 / 2 · 去面包师处 G" : s.Progress == 2 ? "包裹 1 / 2 · 去花匠处 G" : "包裹 2 / 2 · 邮差处 G 免费重玩";
                case RegionalEncounterKind.Prism: return "当前 " + PrismState(s) + " · 目标 绿 / 蓝 / 绿\n靠近棱镜 G 切色；说明牌前 G 复位";
                case RegionalEncounterKind.Egg: return s.Complete ? "龙蛋已安抚，梦影龙战斗保持不变" : "月亮 → 太阳 → 叶片：" + s.Progress + " / 3；然后去软巢 G";
                case RegionalEncounterKind.Bamboo: return s.Active ? "竹桥 / 风灯已展开 · G 收起；地面始终安全" : "靠近金色摇柄 G 展桥、亮灯";
                case RegionalEncounterKind.Sluice: return "熔岩闸门 " + (s.Active ? "开启 · 封闭渠内流动中" : "关闭 · 流动停止") + "\nG 切换；站在护栏外观察熔岩流动";
                case RegionalEncounterKind.Salvage: return "打捞 " + s.Progress + " / 3 · " + (s.Complete ? "G 免费复位" : s.Active ? "绿色带内按 G 收绳！" : "G 启动绞盘，绿色带内再按 G");
                default: return "星光花 " + s.Progress + " / 3 · G 播种 / 浇水 / 采收\n花开需约4秒，三盆完成后说明牌前 G 重种";
            }
        }

        void OnGUI()
        {
            if (game == null || game.Paused || game.PhoneOpen || game.PalaceConsoleOpen || game.Player == null || !Nearest(game.Player.transform.position, 22, out var s, out int node)) return;
            float width = Mathf.Min(470, Screen.width - 32), x = Screen.width - width - 16;
            GUI.Box(new Rect(x, 170, width, s.Kind == RegionalEncounterKind.Salvage && s.Active ? 148 : 118), "");
            var style = new GUIStyle(GUI.skin.label) { font = font, fontSize = 16, wordWrap = true };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(x + 12, 179, width - 24, 100), s.Title + (s.Complete ? " ✓" : "") + "\n" + Hint(s) + "\n最近互动点 " + Mathf.RoundToInt(Vector3.Distance(game.Player.transform.position, s.Nodes[node])) + " 米 · G", style);
            if (s.Kind == RegionalEncounterKind.Salvage && s.Active)
            {
                Rect meter = new Rect(x + 18, 282, width - 36, 16); Color old = GUI.color;
                GUI.color = new Color(.18f, .25f, .31f); GUI.DrawTexture(meter, Texture2D.whiteTexture);
                GUI.color = Mint; GUI.DrawTexture(new Rect(meter.x + meter.width * .35f, meter.y, meter.width * .3f, meter.height), Texture2D.whiteTexture);
                GUI.color = Color.white; GUI.DrawTexture(new Rect(meter.x + meter.width * s.Tension - 2, meter.y - 3, 4, meter.height + 6), Texture2D.whiteTexture); GUI.color = old;
            }
        }
    }

    public partial class Sandbox
    {
        public RegionalEncounters Encounters { get; private set; }
        public void BuildRegionalEncounters(Transform parent)
        {
            if (Encounters != null) { Encounters.gameObject.SetActive(false); Destroy(Encounters.gameObject); }
            var root = new GameObject("地区小故事与互动装置");
            root.transform.SetParent(parent, false);
            Encounters = root.AddComponent<RegionalEncounters>();
            Encounters.Initialize(this);
        }
        public bool TryRegionalEncounter() => Encounters != null && Encounters.Interact();
    }
}
