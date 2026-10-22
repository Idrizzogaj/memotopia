using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class TouchTargetValidation
{
    private static GameObject canvasObject;
    private static GraphicRaycaster raycaster;
    private static EventSystem events;
    private static Button first, second;
    private static Toggle purchase;
    private static RectTransform card;
    private static Camera camera;
    private static RenderTexture texture;

    public static void SetUp()
    {
        events = new GameObject("Touch test events", typeof(EventSystem)).GetComponent<EventSystem>();
        canvasObject = new GameObject("Touch test canvas", typeof(Canvas), typeof(GraphicRaycaster));
        camera = new GameObject("Touch test camera", typeof(Camera)).GetComponent<Camera>();
        camera.orthographic = true;
        texture = new RenderTexture(1024, 768, 24);
        camera.targetTexture = texture;
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        first = Make<Button>("Back", canvasObject.transform, new Vector2(-100, 0), new Vector2(16, 20));
        second = Make<Button>("Neighbour", canvasObject.transform, new Vector2(-65, 0), new Vector2(16, 20));
        card = Make<Image>("Purchase card", canvasObject.transform, new Vector2(100, 0), new Vector2(140, 90)).rectTransform;
        purchase = Make<Toggle>("Purchase contents", card, Vector2.zero, new Vector2(50, 30));
        TouchTarget.Ensure(first); TouchTarget.Ensure(second); TouchTarget.Ensure(purchase, card);
    }

    private static T Make<T>(string name, Transform parent, Vector2 position, Vector2 size) where T : Component
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position;
        return typeof(T) == typeof(Image) ? go.GetComponent<T>() : go.AddComponent<T>();
    }

    private static GameObject Hit(Vector2 point)
    {
        var hits = new List<RaycastResult>();
        raycaster.Raycast(new PointerEventData(events) { position = point }, hits);
        return hits.Count == 0 ? null : ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
    }
    private static Vector2 Point(Transform transform, float x, float y)
    { return RectTransformUtility.WorldToScreenPoint(camera, transform.TransformPoint(new Vector3(x, y))); }
    private static void Require(bool valid, string message)
    { if (!valid) throw new Exception("Touch target: " + message); }

    public static void Validate()
    {
        Canvas.ForceUpdateCanvases();
        foreach (var target in canvasObject.GetComponentsInChildren<TouchTarget>()) target.Refresh();
        Canvas.ForceUpdateCanvases();
        camera.Render();
        Require(Hit(Point(first.transform, -18, 0)) == first.gameObject, "back arrow expanded edge");
        Require(Hit(Point(second.transform, 0, 0)) == second.gameObject, "neighbour keeps visible area");
        Require(Hit(Point(first.transform, 16, 0)) == first.gameObject, "overlap goes to nearest control");
        Require(Hit(Point(second.transform, -16, 0)) == second.gameObject, "opposite overlap ownership");
        first.interactable = false;
        Require(Hit(Point(first.transform, -18, 0)) == null, "disabled target must not expand");
        first.interactable = true;
        second.interactable = false;
        ((RectTransform)second.transform).anchoredPosition = new Vector2(-79, 0);
        second.GetComponentInChildren<TouchTarget>().Refresh();
        Canvas.ForceUpdateCanvases(); camera.Render();
        Require(Hit(Point(first.transform, 18, 0)) != first.gameObject, "disabled neighbour keeps its visible area");
        second.interactable = true;
        ((RectTransform)second.transform).anchoredPosition = new Vector2(-65, 0);
        first.transform.localScale = Vector3.one * 2;
        var enlarged = first.GetComponentInChildren<TouchTarget>();
        enlarged.Refresh();
        var bounds = new Vector3[4];
        ((RectTransform)enlarged.transform).GetWorldCorners(bounds);
        float width = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(camera, bounds[0]), RectTransformUtility.WorldToScreenPoint(camera, bounds[3]));
        Require(width >= 47.9f, "minimum screen size survives scaled layouts");
        first.transform.localScale = Vector3.one;
        enlarged.Refresh();
        int calls = 0;
        purchase.isOn = false;
        purchase.onValueChanged.AddListener(value => { if (value) calls++; });
        var cardEdge = Point(card, 60, 35);
        Require(Hit(cardEdge) == purchase.gameObject, "whole purchase card must be clickable");
        ExecuteEvents.Execute(Hit(cardEdge), new PointerEventData(events) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        Require(purchase.isOn && calls == 1, "purchase card uses existing toggle exactly once");
        var clip = Make<Image>("Scroll viewport", canvasObject.transform, new Vector2(-100, 0), new Vector2(20, 20));
        clip.raycastTarget = false;
        clip.gameObject.AddComponent<RectMask2D>();
        var scroll = clip.gameObject.AddComponent<ScrollRect>();
        first.transform.SetParent(clip.transform, false);
        ((RectTransform)first.transform).anchoredPosition = Vector2.zero;
        enlarged.Refresh();
        Canvas.ForceUpdateCanvases(); camera.Render();
        Require(Hit(Point(first.transform, -18, 0)) == null, "expanded target respects viewport clipping");
        Require(ExecuteEvents.GetEventHandler<IDragHandler>(enlarged.gameObject) == scroll.gameObject, "scroll drag still reaches its parent");
        var group = canvasObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        Require(Hit(cardEdge) == null, "hidden canvas must not intercept touches");
        UnityEngine.Object.Destroy(canvasObject);
        UnityEngine.Object.Destroy(events.gameObject);
        camera.targetTexture = null;
        UnityEngine.Object.Destroy(camera.gameObject);
        UnityEngine.Object.Destroy(texture);
        Debug.Log("TOUCH_TARGET_VALIDATION_PASSED: expanded back, neighbours, overlap, disabled, scaled layout, whole card, single toggle, clipping, scroll, canvas group.");
    }
}
