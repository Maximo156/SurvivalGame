using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour, IDragHandler, IBeginDragHandler //, IEndDragHandler
{
    public Image islandImage;
    public Island Island;

    public int buffer;
    public Color WaterColor;

    RectTransform rect;

    public Transform player;
    public RectTransform playerIcon;
    

    // Start is called before the first frame update
    void Start()
    {
        GenerateImage();
    }

    private void OnEnable()
    {
        rect = GetComponent<RectTransform>();

        for (int i = 0; i<transform.parent.childCount; i++)
        {
            if(transform.parent.GetChild(i) != transform)
            {
                transform.parent.GetChild(i).gameObject.SetActive(false);
            }
        }

        playerIcon.localScale = new Vector3(1, 1, 1f);
        transform.localScale = new Vector3(1, 1, 1f);
        transform.localPosition = Vector3.zero;
    }

    public float panSpeed = 0.1f;
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.1f;
    public float maxZoom = 5f;

    private void Update()
    {
        // Zoom In/Out
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        float newScale = transform.localScale.x + scrollValue * zoomSpeed;
        newScale = Mathf.Clamp(newScale, minZoom, maxZoom);
        if (newScale != transform.localScale.x)
        {
            transform.localScale = new Vector3(newScale, newScale, 1f);
            playerIcon.localScale = new Vector3(1/newScale, 1/newScale, 1f);
        }

        
    }

    private void FixedUpdate()
    {
        playerIcon.localPosition = WorldToLocal(player.position);
    }

    Vector3 dragOffset;
    public void OnBeginDrag(PointerEventData data)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(data.pointerEnter.transform as RectTransform, data.position, data.pressEventCamera, out var globalMousePos))
        {
            dragOffset = rect.position - globalMousePos;
            // LockableInput.Rotation = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        var rectTrans = data?.pointerEnter?.transform as RectTransform;
        if (rectTrans != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTrans, data.position, data.pressEventCamera, out var globalMousePos))
        {
            rect.position = globalMousePos + dragOffset;
        }
    }

    /*
    public void OnEndDrag(PointerEventData eventData)
    {
        LockableInput.Rotation = true;
    }*/

    int width;
    int height;
    int minDim;

    public void GenerateImage()
    {
        height = (int)islandImage.rectTransform.rect.height;
        width = (int)islandImage.rectTransform.rect.width;

        minDim = Mathf.Min(height, width);

        Texture2D text = new Texture2D(width, height, TextureFormat.ARGB32, false);
        
        Color[] colors = new Color[height * width];

        for(int i = 0; i < height; i++)
        {
            for(int k = 0; k<width; k++)
            {
                colors[k + i * width] = GenColor(k, i);
            }
        }

        text.SetPixels(colors);
        text.Apply();

        Sprite sprite = Sprite.Create(text, new Rect(0f, 0f, text.width, text.height), new Vector2(0f, 1f));

        islandImage.sprite = sprite;
    }

    public Vector2 PixelToWorld(int w, int h)
    {
        var res = new Vector2(w - width / 2f, h - height / 2f) * Island.islandWidth / (minDim - buffer);

        return res;
    }

    public Vector3 WorldToLocal(Vector3 pos)
    {
        pos = new Vector3(pos.x * (minDim - buffer) / Island.islandWidth, pos.z * (minDim - buffer) / Island.islandWidth, 0);

        return pos;
    }

    public float noiseScale;
    public Color GenColor(int w, int h)
    {
        var pos = PixelToWorld(w, h);
        float height = Island.sound.GetHeight(new Vector3(pos.x, 0,pos.y));

        Color color;

        if (height < Island.WaterLevel)
        {
            color = WaterColor;

            var alpha = 1 - Mathf.Pow(new Vector2((w - width / 2f) * minDim / width, h - this.height / 2f).magnitude / (minDim / 2f), 3);

            alpha = alpha - (1 - alpha) * Mathf.PerlinNoise(w * noiseScale, h * noiseScale) * 0.5f;

            color.a = alpha;

            return color;
        }
        else if (height < Island.SandStop)
            color = Island.SandColor;
        else if (height < Island.GrassStop)
            color = Island.GrassColor;
        else if (height < Island.RockStop)
            color = Island.RockColor;
        else
            color = Island.SnowColor;

        color.a = 0.8f;

        return color;
    }

}
