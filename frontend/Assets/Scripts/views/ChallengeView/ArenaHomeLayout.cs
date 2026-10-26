using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Separate the original search, added actions and existing scrolling player list.
public static class ArenaHomeLayout {
 sealed class PlayerRow {public Transform row;public int id;}
 static readonly List<PlayerRow> players=new List<PlayerRow>();
 public static void Forget(Transform content){players.RemoveAll(p=>p.row==null || p.row.parent==content);}
 public static void Track(Transform row,Text label,Button sword,int id){players.RemoveAll(p=>p.row==null);players.Add(new PlayerRow{row=row,id=id});PresenceIndicator.BeforeButton(label,sword);}
 public static void RefreshPlayers(Transform content){
  if(content==null)return;players.RemoveAll(p=>p.row==null);
  var rows=players.Where(p=>p.row.parent==content).ToArray();if(rows.Length==0)return;
  int index=0;
  foreach(bool online in new[]{true,false}){
   var group=rows.Where(p=>LiveChallenges.IsOnline(p.id)==online).ToArray();string name=online?"Online players divider":"Offline players divider";var line=content.Find(name);
   if(group.Length==0){if(line!=null)line.gameObject.SetActive(false);continue;}
   if(line==null){var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(LayoutElement));go.transform.SetParent(content,false);line=go.transform;var r=(RectTransform)line;r.sizeDelta=new Vector2(0,3);var size=go.GetComponent<LayoutElement>();size.preferredHeight=size.minHeight=3;size.flexibleHeight=0;go.GetComponent<Image>().raycastTarget=false;}
   line.gameObject.SetActive(true);line.GetComponent<Image>().color=online?(Color)new Color32(28,145,89,255):(Color)new Color32(202,48,75,255);line.SetSiblingIndex(index++);
   foreach(var row in group)row.row.SetSiblingIndex(index++);
  }
 }
 static void Place(RectTransform rect,Vector2 min,Vector2 max){if(rect==null)return;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
 public static void Apply(ChallengeNavigation navigation){
  var home=navigation.challengeHomeScreen.transform;
  if(navigation.searchInput!=null)Place(navigation.searchInput.GetComponent<RectTransform>(),new Vector2(.06f,.80f),new Vector2(.94f,.865f));
  var challenges=home.Find("Challenges");if(challenges!=null)Place(challenges as RectTransform,new Vector2(.12f,.71f),new Vector2(.88f,.77f));
  var online=home.Find("Online");if(online!=null)Place(online as RectTransform,new Vector2(.12f,.635f),new Vector2(.88f,.695f));
  Place(navigation.FavoritesScrollView.transform as RectTransform,new Vector2(0,.135f),new Vector2(1,.61f));
  if(navigation.NoFavoritesScreen!=null)Place(navigation.NoFavoritesScreen.transform as RectTransform,new Vector2(0,.135f),new Vector2(1,.61f));
  var scroll=navigation.FavoritesScrollView.GetComponent<ScrollRect>();if(scroll!=null){scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;if(scroll.horizontalScrollbar!=null)scroll.horizontalScrollbar.gameObject.SetActive(false);}
  var content=navigation.favoriteContent;
  var layout=content.GetComponent<VerticalLayoutGroup>();
  if(layout==null){layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=true;layout.childControlHeight=false;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;}
  layout.padding.top=Mathf.Max(20,layout.padding.top);layout.padding.bottom=Mathf.Max(20,layout.padding.bottom);
  var fitter=content.GetComponent<ContentSizeFitter>()??content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
  var random=home.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.transform.parent==home && b.GetComponentsInChildren<Text>(true).Any(t=>t.text.ToLowerInvariant().Contains("random")));
  if(random!=null)Place(random.transform as RectTransform,new Vector2(.083f,.03f),new Vector2(.917f,.105f));
 }
}
