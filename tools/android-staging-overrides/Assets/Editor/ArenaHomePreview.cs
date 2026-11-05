using System.Linq;
using System.Reflection;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class ArenaHomePreview {
 public static void Render(){
  EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var nav=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().First(n=>n.gameObject.scene.IsValid());SprintLobby.Attach(nav);
  var canvas=nav.challengeHomeScreen.GetComponentsInParent<Canvas>(true).First().rootCanvas;
  for(var t=nav.challengeHomeScreen.transform;t!=canvas.transform;t=t.parent){foreach(Transform sibling in t.parent)if(sibling!=t)sibling.gameObject.SetActive(false);t.gameObject.SetActive(true);}
  var users=Enumerable.Range(1,8).Select(i=>new User{id=100+i,username="Player "+i,userStatistics=new UserStatistics{level=i,xp=i*70}}).ToArray();nav.SetFavorites(new UserSerializable{users=users});nav.NoChallenge.SetActive(false);
  var avatar=Resources.LoadAll<Sprite>("Memotopia_UI/AccountAndSettings").FirstOrDefault(s=>Mathf.Approximately(s.rect.width,s.rect.height));foreach(Transform row in nav.favoriteContent){var image=row.GetComponentInChildren<Image>();if(image!=null && avatar!=null)image.sprite=avatar;}
  canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;canvas.GetComponent<CanvasScaler>().enabled=false;var r=(RectTransform)canvas.transform;r.sizeDelta=new Vector2(800,1734);
  var camera=new GameObject("Arena preview camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.cullingMask=1<<30;camera.backgroundColor=Color.white;camera.clearFlags=CameraClearFlags.SolidColor;canvas.worldCamera=camera;
  var capture=typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static);Canvas.ForceUpdateCanvases();foreach(var badge in canvas.GetComponentsInChildren<PresenceIndicator>())PresenceIndicator.Set(badge.transform.parent.GetComponent<Text>(),false);capture.Invoke(null,new object[]{canvas.gameObject,camera,"arena-home-spacing-phone",945,2048});r.sizeDelta=new Vector2(800,1067);Canvas.ForceUpdateCanvases();foreach(var badge in canvas.GetComponentsInChildren<PresenceIndicator>())PresenceIndicator.Set(badge.transform.parent.GetComponent<Text>(),false);capture.Invoke(null,new object[]{canvas.gameObject,camera,"arena-home-spacing-ipad",1024,1366});Debug.Log("ARENA_HOME_PREVIEW_RENDERED");
 }
}
