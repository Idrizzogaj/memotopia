using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Separate the original search, added actions and existing scrolling player list.
public static class ArenaHomeLayout {
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
