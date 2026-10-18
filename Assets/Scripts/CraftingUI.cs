using UnityEngine;

namespace Blocklands
{
    public partial class Sandbox
    {
        int craftingCursor,recipeHint,bagPage;
        const int ItemsPerPage=14,RecipesPerPage=6;
        int BagPages=>(Blocks.BagItems.Length+ItemsPerPage-1)/ItemsPerPage;
        int RecipePages=>(Blocklands.Crafting.Recipes.Length+RecipesPerPage-1)/RecipesPerPage;
        public void ShowRecipe(int index){recipeHint=Mathf.Clamp(index,0,Blocklands.Crafting.Recipes.Length-1);}
        public void ShowBagPage(int page){bagPage=(page+BagPages)%BagPages;}
        static readonly Color Paper=new Color(.91f,.89f,.82f),Ink=new Color(.17f,.23f,.22f),Muted=new Color(.37f,.42f,.39f),Gold=new Color(.73f,.49f,.17f);
        void CraftingKeys()
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))craftingCursor=craftingCursor%3>0?craftingCursor-1:craftingCursor;
            if(Input.GetKeyDown(KeyCode.RightArrow))craftingCursor=craftingCursor%3<2?craftingCursor+1:craftingCursor;
            if(Input.GetKeyDown(KeyCode.UpArrow))craftingCursor=Mathf.Max(0,craftingCursor-3);
            if(Input.GetKeyDown(KeyCode.DownArrow))craftingCursor=Mathf.Min(8,craftingCursor+3);
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                int i=System.Array.IndexOf(Blocks.BagItems,Selected);i=(i+1)%Blocks.BagItems.Length;SelectItem(Blocks.BagItems[i]);bagPage=i/ItemsPerPage;
            }
            if(Input.GetKeyDown(KeyCode.Space))PlaceIngredient(craftingCursor);
            if(Input.GetKeyDown(KeyCode.Backspace)||Input.GetKeyDown(KeyCode.Delete))Crafting.SetCell(craftingCursor,Block.Air,Inventory);
            if(Input.GetKeyDown(KeyCode.C)){Crafting.Clear();Notice="摆放已清空，材料仍在背包中。";}
            if(Input.GetKeyDown(KeyCode.Return))CollectCraft();
            if(Input.GetKeyDown(KeyCode.Q))recipeHint=(recipeHint+1)%Blocklands.Crafting.Recipes.Length;
            if(Input.GetKeyDown(KeyCode.PageDown))ShowRecipe(((recipeHint/RecipesPerPage+1)%RecipePages)*RecipesPerPage);
            if(Input.GetKeyDown(KeyCode.PageUp))ShowRecipe(((recipeHint/RecipesPerPage+RecipePages-1)%RecipePages)*RecipesPerPage);
            if(Input.GetKeyDown(KeyCode.LeftBracket))ShowBagPage(bagPage-1);
            if(Input.GetKeyDown(KeyCode.RightBracket))ShowBagPage(bagPage+1);
        }
        void PlaceIngredient(int index)
        {
            if(!Crafting.SetCell(index,(Block)Selected,Inventory))Notice="这种材料不够了，先从其他格取回，或者去采集。";
            else Notice="已摆入 1 个"+Blocks.Names[Selected]+"。领取结果时才消耗材料。";
        }
        void DrawCrafting(float w,float h)
        {
            float x=(w-944)/2,y=(h-610)/2;
            if(Event.current.type==EventType.MouseDown&&HandleCraftingPointer(Event.current.mousePosition,Event.current.button,w,h))Event.current.Use();
            Panel(new Rect(x+7,y+7,944,610),new Color(0,0,0,.3f));Panel(new Rect(x,y,944,610),Paper);
            Panel(new Rect(x,y,944,6),Gold);
            Label(new Rect(x+28,y+19,260,42),"书包 / 合成",29,Ink);
            if(FlatButton(new Rect(x+296,y+23,260,38),"派对与游乐用品（7种）",true)){OpenPartyBag();return;}
            if(FlatButton(new Rect(x+561,y+23,220,38),"拿摄像头 × "+Inventory[94],false)){TakeCameraFromBag();return;}
            if(FlatButton(new Rect(x+798,y+23,120,38),"关闭  [E]",false)){SetPaused(false);return;}
            Label(new Rect(x+28,y+65,850,28),"先选背包材料 → 点格子放 1 个 → 按配方摆好 → 点击结果领取",16,Muted);
            if(FlatButton(new Rect(x+798,y+65,120,28),"空手 / 放下",false)){PutHeldItemAway();SetPaused(false);return;}
            Panel(new Rect(x+612,y+109,1,450),new Color(.72f,.73f,.66f));
            Label(new Rect(x+28,y+105,278,30),"材料摆放  3 × 3",19,Ink);
            Label(new Rect(x+310,y+107,275,28),"音响、彩灯、舞台、秋千、滑梯在上方",13,Muted);
            for(int i=0;i<9;i++)
            {
                Rect cell=CraftingCellRect(w,h,i);
                Panel(cell,i==craftingCursor?Gold:new Color(.69f,.71f,.66f));
                Panel(new Rect(cell.x+3,cell.y+3,74,74),new Color(.8f,.81f,.75f));
                Block item=Crafting.Grid[i];
                if(item!=Block.Air){ItemIcon(new Rect(cell.x+25,cell.y+13,30,30),item);Label(new Rect(cell.x,cell.y+51,80,25),Blocks.Names[(int)item],14,Ink,TextAnchor.MiddleCenter);}
                else Label(new Rect(cell.x+8,cell.y+5,24,22),(i+1).ToString(),12,Muted);
            }
            Label(new Rect(x+296,y+230,40,42),"→",32,Gold,TextAnchor.MiddleCenter);
            var match=Crafting.Match();bool ready=Crafting.CanCollect(Inventory);
            Label(new Rect(x+355,y+144,225,30),"合成结果",19,Ink);
            Rect output=new Rect(x+355,y+183,112,106);
            Panel(output,ready?new Color(.63f,.72f,.53f):new Color(.75f,.77f,.7f));
            if(match!=null){ItemIcon(new Rect(output.x+36,output.y+15,40,40),match.Output);Label(new Rect(output.x,output.y+66,112,29),match.Name+" × "+match.Count,16,Ink,TextAnchor.MiddleCenter);}
            else Label(output,"？",38,Muted,TextAnchor.MiddleCenter);
            if(Event.current.type==EventType.MouseDown&&Event.current.button==0&&output.Contains(Event.current.mousePosition)){CollectCraft();Event.current.Use();}
            bool wasEnabled=GUI.enabled;GUI.enabled=ready;
            if(FlatButton(new Rect(x+355,y+304,218,40),"领取结果  [Enter]",true))CollectCraft();GUI.enabled=wasEnabled;
            var feedback=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};feedback.normal.textColor=Muted;
            GUI.Label(new Rect(x+355,y+352,230,66),Notice,feedback);
            if(FlatButton(new Rect(x+28,y+407,175,32),"清空格子  [C]",false))Crafting.Clear();
            if(FlatButton(new Rect(x+219,y+407,45,32),"[ ←",false))ShowBagPage(bagPage-1);
            Label(new Rect(x+271,y+411,242,30),"背包 "+(bagPage+1)+" / "+BagPages+" 页 · Tab选材",14,Ink);
            if(FlatButton(new Rect(x+535,y+407,53,32),"→ ]",false))ShowBagPage(bagPage+1);
            for(int n=0;n<ItemsPerPage;n++)
            {
                int index=bagPage*ItemsPerPage+n;if(index>=Blocks.BagItems.Length)break;
                int id=Blocks.BagItems[index];Rect cell=new Rect(x+28+(n%7)*80,y+449+(n/7)*61,74,55);
                if(FlatButton(cell,"",Selected==id))SelectItem(id);
                ItemIcon(new Rect(cell.x+7,cell.y+7,19,19),(Block)id);
                Label(new Rect(cell.x+30,cell.y+5,42,25),Crafting.Available((Block)id,Inventory).ToString(),13,Ink,TextAnchor.MiddleCenter);
                Label(new Rect(cell.x,cell.y+29,74,23),id==94?"监控摄像头":Blocks.Names[id],13,Ink,TextAnchor.MiddleCenter);
            }
            // A recipe guide shows positions but never crafts on the player's behalf.
            Panel(new Rect(x+630,y+101,290,470),new Color(.96f,.92f,.79f));Panel(new Rect(x+630,y+101,5,470),Gold);
            Label(new Rect(x+645,y+108,270,30),"配方小本子 · 全 "+Blocklands.Crafting.Recipes.Length+" 种",18,Ink);
            int recipePage=recipeHint/RecipesPerPage;
            for(int n=0;n<RecipesPerPage;n++)
            {
                int i=recipePage*RecipesPerPage+n;if(i>=Blocklands.Crafting.Recipes.Length)break;
                var r=Blocklands.Crafting.Recipes[i];
                if(FlatButton(new Rect(x+645,y+146+n*30,261,26),r.Name+" × "+r.Count,i==recipeHint))recipeHint=i;
            }
            if(FlatButton(new Rect(x+645,y+331,62,27),"←上页",false))ShowRecipe(((recipePage+RecipePages-1)%RecipePages)*RecipesPerPage);
            Label(new Rect(x+711,y+333,121,26),(recipePage+1)+" / "+RecipePages+" 页",14,Ink,TextAnchor.MiddleCenter);
            if(FlatButton(new Rect(x+843,y+331,62,27),"下页→",false))ShowRecipe(((recipePage+1)%RecipePages)*RecipesPerPage);
            var hint=Blocklands.Crafting.Recipes[recipeHint];
            var hintStyle=new GUIStyle(GUI.skin.label){fontSize=14,wordWrap=true};hintStyle.normal.textColor=Ink;
            GUI.Label(new Rect(x+645,y+363,261,45),hint.Hint,hintStyle);
            for(int i=0;i<9;i++)
            {
                int col=i%3,row=i/3;Rect cell=new Rect(x+674+col*58,y+412+row*48,52,42);
                Panel(cell,new Color(.8f,.81f,.75f));
                if(col<hint.Width&&row<hint.Height){Block b=hint.Pattern[row*hint.Width+col];if(b!=Block.Air){ItemIcon(new Rect(cell.x+5,cell.y+5,15,15),b);Label(new Rect(cell.x,cell.y+20,52,22),Blocks.Names[(int)b],11,Ink,TextAnchor.MiddleCenter);}}
            }
            Label(new Rect(x+28,y+575,580,29),"选格空格放入 · 删除取回 · R吃食物 · "+Blocks.Names[Selected],14,Muted);
            // Keep transient feedback readable without covering either grid.
            Label(new Rect(x+639,y+573,274,27),"Q 下一配方 · PgUp/PgDn翻页",12,Muted);
        }
        static bool FlatButton(Rect r,string text,bool selected)
        {
            Panel(r,selected?new Color(.72f,.78f,.61f):new Color(.84f,.85f,.79f));
            var style=new GUIStyle(GUI.skin.button){fontSize=15,border=new RectOffset(),padding=new RectOffset(5,5,2,2)};
            style.normal.background=null;style.hover.background=null;style.active.background=null;
            style.normal.textColor=Ink;style.hover.textColor=Ink;style.active.textColor=Ink;
            return GUI.Button(r,text,style);
        }
        static void ItemIcon(Rect r,Block b)
        {
            Color c=Blocks.ColorOf(b);
            if(b==Blocks.SurveillanceCamera){Panel(new Rect(r.x,r.y+3,r.width,r.height-5),new Color(.78f,.84f,.89f));Panel(new Rect(r.x+r.width*.32f,r.y+r.height*.25f,r.width*.5f,r.height*.5f),new Color(.06f,.18f,.24f));Panel(new Rect(r.x+r.width*.42f,r.y+r.height*.33f,r.width*.3f,r.height*.3f),new Color(.25f,.75f,.97f));return;}
            if(b==Block.Coin){Panel(new Rect(r.x,r.y+r.height*.25f,r.width,r.height*.5f),new Color(.82f,.94f,.78f));Panel(new Rect(r.x+2,r.y+r.height*.3f,r.width-4,r.height*.4f),new Color(.35f,.64f,.45f));Label(new Rect(r.x,r.y+r.height*.24f,r.width,r.height*.55f),"1",Mathf.Max(9,Mathf.RoundToInt(r.height*.5f)),Color.white,TextAnchor.MiddleCenter);return;}
            if(b==Blocks.InvitationCard){Panel(new Rect(r.x,r.y+r.height*.15f,r.width,r.height*.7f),new Color(1,.96f,.82f));Panel(new Rect(r.x+2,r.y+r.height*.25f,r.width-4,3),new Color(.96f,.52f,.71f));return;}
            if(b==Block.Stick){Panel(new Rect(r.x+r.width*.38f,r.y,r.width*.23f,r.height),c);return;}
            if(Blocks.Tool(b))
            {
                Panel(new Rect(r.x+r.width*.42f,r.y+r.height*.15f,r.width*.16f,r.height*.85f),Blocks.ColorOf(Block.Stick));
                Panel(new Rect(r.x,r.y,r.width,r.height*.27f),c);Panel(new Rect(r.x,r.y,r.width*.16f,r.height*.45f),c);return;
            }
            Panel(new Rect(r.x,r.y+3,r.width,r.height-3),c*.75f);Panel(new Rect(r.x,r.y,r.width-4,r.height-5),c);
            Panel(new Rect(r.x+2,r.y+2,r.width-7,3),Color.Lerp(c,Color.white,.28f));
            if(b==Block.Workbench)for(int i=1;i<3;i++){Panel(new Rect(r.x+i*r.width/3,r.y,1,r.height-4),Ink);Panel(new Rect(r.x,r.y+i*r.height/3,r.width-4,1),Ink);}
        }
    }
}
