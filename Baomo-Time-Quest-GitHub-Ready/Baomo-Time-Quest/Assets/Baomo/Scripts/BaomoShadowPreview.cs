using UnityEngine;
public class BaomoShadowPreview : MonoBehaviour {
 public Texture2D background, hat, sun;
 public Texture2D projection;
 public Texture2D successPopup;
 public Texture2D avatarIcon, modeMapIcon, compassIcon, gardenMap;
 public string nickname="Xiaolin";
 bool mapOpen, observationMode=true;
 bool rewardOpen; float rewardStart=-1;
 [Range(0,1)] public float sunPosition=.5f;
 public float correctPosition=.18f;
 public float tolerance=.035f;
 bool dragging, won; string message="Drag the sun to Mao Hour (5-7 AM) and align the projection with the hat.";
 Vector2 P(float x,float y){return new Vector2(x,y);}
 void Line(Vector2 a,Vector2 b,Color c,float width=2f){
  var old=GUI.matrix; var color=GUI.color; Vector2 d=b-a;
  GUI.color=c; GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg,a);
  GUI.DrawTexture(new Rect(a.x,a.y,d.magnitude,width),Texture2D.whiteTexture);
  GUI.matrix=old; GUI.color=color;
 }
 Vector2 SunPoint(float t){return P(Mathf.Lerp(235,1460,t),270-Mathf.Sin(t*Mathf.PI)*185);}
 public bool Aligned(){return Mathf.Abs(sunPosition-correctPosition)<=tolerance;}
 void OnGUI(){
  var previous=GUI.matrix;
  float s=Mathf.Min(Screen.width/1704f,Screen.height/786f);
  GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1704*s)/2,(Screen.height-786*s)/2,0),Quaternion.identity,new Vector3(s,s,1));
  if(background)GUI.DrawTexture(new Rect(0,0,1704,786),background,ScaleMode.StretchToFill);
  if(mapOpen){DrawMap(); GUI.matrix=previous;return;}
  if(rewardOpen){ DrawReward(); GUI.matrix=previous; return; }
  DrawHUD();
  if(rewardStart>=0) DrawFragments();
  bool hudEnabled=GUI.enabled;GUI.enabled=observationMode;
  var gold=new Color(1,.83f,.38f,.9f);
  for(int i=0;i<80;i++)Line(SunPoint(i/80f),SunPoint((i+1)/80f),gold,2);
  Vector2 sp=SunPoint(sunPosition);
  var e=Event.current;
  if(observationMode && !won && e.type==EventType.MouseDown && new Rect(sp.x-65,sp.y-65,130,130).Contains(e.mousePosition)){dragging=true;e.Use();}
  if(observationMode && dragging && (e.type==EventType.MouseDrag || e.type==EventType.MouseUp)){
   sunPosition=Mathf.Clamp01((e.mousePosition.x-235)/1225);
   if(e.type==EventType.MouseUp)dragging=false; e.Use();
  }
  if(sun)GUI.DrawTexture(new Rect(sp.x-58,sp.y-58,116,116),sun,ScaleMode.ScaleToFit);
  else {GUI.color=gold;GUI.Box(new Rect(sp.x-28,sp.y-28,56,56),"");GUI.color=Color.white;}
  // A textured 2D projection deforms with the virtual sun.
  float delta=sunPosition-correctPosition;
  if(projection){
   var saved=GUI.matrix;
   var warp=Matrix4x4.identity;
   warp.m01=delta*.55f;
   warp.m00=1+Mathf.Abs(delta)*.22f;
   warp.m11=1+delta*.65f;
   GUI.matrix=saved*Matrix4x4.Translate(new Vector3(570+delta*190,610,0))*warp;
   var tint=GUI.color;
   GUI.color=new Color(1,1,1,.78f);
   GUI.DrawTexture(new Rect(0,0,660,161),projection,ScaleMode.StretchToFill,true);
   GUI.color=tint;
   GUI.matrix=saved;
  }
  if(hat)GUI.DrawTexture(new Rect(570,677,660,94),hat,ScaleMode.ScaleToFit);
  var style=new GUIStyle(GUI.skin.box){fontSize=25,alignment=TextAnchor.MiddleCenter};
  GUI.Box(new Rect(1240,270,400,55),Aligned()?"Mao Hour - Aligned":"Move the sun - Find Mao Hour",style);
  GUI.Box(new Rect(250,560,1200,62),message,style);
  if(GUI.Button(new Rect(1500,690,160,65),"Confirm",new GUIStyle(GUI.skin.button){fontSize=28})){
   won=Aligned(); if(won){rewardOpen=true;dragging=false;rewardStart=Time.unscaledTime;} message=won?"Baomo Hall - Level complete!":"Not aligned yet. Keep moving the sun.";
  }
  if(GUI.Button(new Rect(25,690,150,65),"Restart",new GUIStyle(GUI.skin.button){fontSize=24})){won=false;rewardOpen=false;rewardStart=-1;sunPosition=.5f;message="Drag the sun to Mao Hour (5-7 AM) and align the projection with the hat.";}
  GUI.enabled=hudEnabled;
  GUI.Label(new Rect(250,20,950,38),"Baomo Hall - Photo Preview (AR not connected)",new GUIStyle(GUI.skin.box){fontSize=22});
  GUI.matrix=previous;
 }

 void DrawFragments(){
  float age=Time.unscaledTime-rewardStart;
  if(age<0 || age>3.2f || !projection)return;
  Color original=GUI.color;
  for(int i=0;i<18;i++){
   float t=Mathf.Max(0,age-(i%6)*.07f);
   float x=852+((i%6)-2.5f)*95+Mathf.Sin(i*2.4f)*t*65;
   float y=130+(i/6)*45+45*t+100*t*t;
   var matrix=GUI.matrix;
   GUIUtility.RotateAroundPivot((i%2==0?1:-1)*t*(25+i*4),new Vector2(x+40,y+25));
   GUI.color=new Color(1,1,1,Mathf.Clamp01((3.2f-age)/.7f));
   GUI.DrawTextureWithTexCoords(new Rect(x,y,85,50),projection,new Rect((i%6)/6f,(i/6)/3f,1f/6,1f/3),true);
   GUI.matrix=matrix;
  }
  GUI.color=original;
 }
 void DrawReward(){
  Color original=GUI.color;
  GUI.color=new Color(0,0,0,.55f);GUI.DrawTexture(new Rect(0,0,1704,786),Texture2D.whiteTexture);GUI.color=original;
  if(successPopup)GUI.DrawTexture(new Rect(284,150,1136,458),successPopup,ScaleMode.StretchToFill,true);
  // English text covers the text baked into the supplied popup.
  GUI.color=new Color(.94f,.92f,.85f,1);
  GUI.DrawTexture(new Rect(890,215,365,305),Texture2D.whiteTexture);
  GUI.DrawTexture(new Rect(807,541,86,25),Texture2D.whiteTexture);
  GUI.DrawTexture(new Rect(1080,530,165,51),Texture2D.whiteTexture);
  GUI.color=original;
  var title=new GUIStyle(GUI.skin.label){fontSize=31,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
  title.normal.textColor=new Color(.02f,.35f,.4f);
  GUI.Label(new Rect(895,220,355,45),"Wok-Ear Gable",title);
  var body=new GUIStyle(GUI.skin.label){fontSize=21,wordWrap=true};
  body.normal.textColor=new Color(.15f,.25f,.29f);
  GUI.Label(new Rect(905,280,335,235),"A traditional Guangdong gable named for its wok-handle shape. Built from blue bricks with decorative crests, it helps block fire and wind. Its official-hat silhouette once symbolized status and hopes for scholarly success. It remains an emblem of Lingnan architecture.",body);
  DrawFragments();
  // Transparent hit area follows the confirm button already painted in the supplied artwork.
  if(GUI.Button(new Rect(1080,520,165,65),"Confirm",new GUIStyle(GUI.skin.button){fontSize=25})){
   rewardOpen=false;message="Fragment collected: Wok-Ear Gable";
  }
 }

 void DrawHUD(){
  if(avatarIcon)GUI.DrawTexture(new Rect(48,55,104,102),avatarIcon,ScaleMode.ScaleToFit,true);
  var nameStyle=new GUIStyle(GUI.skin.textField){fontSize=22,alignment=TextAnchor.MiddleCenter};
  nameStyle.normal.textColor=new Color(.02f,.4f,.45f);
  nickname=GUI.TextField(new Rect(28,158,145,30),nickname,18,nameStyle);
  if(modeMapIcon)GUI.DrawTexture(new Rect(40,190,124,274),modeMapIcon,ScaleMode.ScaleToFit,true);
  if(GUI.Button(new Rect(49,195,106,127),GUIContent.none,GUIStyle.none)){
   observationMode=!observationMode;dragging=false;
  }
  if(GUI.Button(new Rect(49,325,106,128),GUIContent.none,GUIStyle.none)){
   mapOpen=true;dragging=false;
  }
  if(compassIcon)GUI.DrawTexture(new Rect(1460,30,222,220),compassIcon,ScaleMode.ScaleToFit,true);
  var label=new GUIStyle(GUI.skin.box){fontSize=18,alignment=TextAnchor.MiddleCenter};
  GUI.Label(new Rect(38,470,140,30),observationMode?"Scope mode":"View mode",label);
 }
 void DrawMap(){
  var color=GUI.color;GUI.color=new Color(0,0,0,.72f);
  GUI.DrawTexture(new Rect(0,0,1704,786),Texture2D.whiteTexture);GUI.color=color;
  if(gardenMap)GUI.DrawTexture(new Rect(200,65,1304,650),gardenMap,ScaleMode.ScaleToFit,true);
  GUI.Label(new Rect(600,15,504,45),"Baomo Garden Map",new GUIStyle(GUI.skin.box){fontSize=28,alignment=TextAnchor.MiddleCenter});
  if(GUI.Button(new Rect(1450,25,210,55),"Back to garden",new GUIStyle(GUI.skin.button){fontSize=23}))mapOpen=false;
 }
}