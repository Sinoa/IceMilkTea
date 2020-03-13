using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestAnimation : BaseMeshEffect
{
    public enum TimeMode
    {
        Time,
        Speed,
    }

    public enum Style
    {
        Once,
        Loop,
        PingPong,
    }

    public TimeMode timeMode = TimeMode.Time;
    public float duration = 1.0f;
    public float speed = 5.0f;
    public float delay = 0;
    public Style style = Style.Once;
    public bool playOnAwake = true;
    [SerializeField] float m_progress = 1.0f;
    public bool usePosition = false;
    public bool useRotation = false;
    public bool useScale = false;
    public bool useAlpha = false;
    public bool useColor = false;
    public UnityEvent onStart;
    public UnityEvent onComplete;
    [SerializeField] int characterNumber = 0;
    float animationTime = 0;
    Coroutine playCoroutine = null;
    bool m_isPlaying = false;

    public Vector3 positionFrom = Vector3.zero;
    public Vector3 positionTo = Vector3.zero;
    public AnimationCurve positionAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float positionSeparation = 0.5f;

    public float rotationFrom = 0;
    public float rotationTo = 0;
    public Vector2 rotationPivot = new Vector2(0.5f, 0.5f);
    public AnimationCurve rotationAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float rotationSeparation = 0.5f;

    public bool scaleSyncXY = true;
    public float scaleFrom = 0;
    public float scaleTo = 1.0f;
    public Vector2 scalePivot = new Vector2(0.5f, 0.5f);
    public AnimationCurve scaleAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float scaleFromY = 0;
    public float scaleToY = 1.0f;
    public Vector2 scalePivotY = new Vector2(0.5f, 0.5f);
    public AnimationCurve scaleAnimationCurveY = AnimationCurve.Linear(0, 0, 1, 1);
    public float scaleSeparation = 0.5f;

    public float alphaFrom = 0.0f;
    public float alphaTo = 1.0f;
    public AnimationCurve alphaAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float alphaSeparation = 0.5f;

    public Color colorFrom = Color.white;
    public Color colorTo = Color.white;
    public AnimationCurve colorAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float colorSeparation = 0.5f;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        progress = m_progress;
        base.OnValidate();
    }
#endif

    public float progress
    {
        get { return m_progress; }
        set
        {
            m_progress = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    public bool isPlaying
    {
        get { return m_isPlaying; }
    }

    protected override void OnEnable()
    {
        if (playOnAwake) Play();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        Stop();
        base.OnDisable();
    }

    public void Play()
    {
        progress = 0;
        switch (timeMode)
        {
            case TimeMode.Time:
                animationTime = duration;
                break;
            case TimeMode.Speed:
                animationTime = characterNumber / 10.0f / speed;
                break;
        }

        switch (style)
        {
            case Style.Once:
                playCoroutine = StartCoroutine(PlayOnceCoroutine());
                break;
            case Style.Loop:
                playCoroutine = StartCoroutine(PlayLoopCoroutine());
                break;
            case Style.PingPong:
                playCoroutine = StartCoroutine(PlayPingPongCoroutine());
                break;
        }
    }

    public void Stop()
    {
        if (playCoroutine != null) StopCoroutine(playCoroutine);
        m_isPlaying = false;
        playCoroutine = null;
    }

    IEnumerator PlayOnceCoroutine()
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (m_isPlaying) { yield break; }
        m_isPlaying = true;
        if (onStart != null) onStart.Invoke();

        while (progress < 1.0f)
        {
            progress += Time.deltaTime / animationTime;
            yield return null;
        }

        m_isPlaying = false;
        progress = 1.0f;
        if (onComplete != null) onComplete.Invoke();
    }

    IEnumerator PlayLoopCoroutine()
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (m_isPlaying) { yield break; }
        m_isPlaying = true;
        if (onStart != null) onStart.Invoke();

        while (true)
        {
            progress += Time.deltaTime / animationTime;
            if (progress > 1.0f)
            {
                progress -= 1.0f;
            }
            yield return null;
        }
    }

    IEnumerator PlayPingPongCoroutine()
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        if (m_isPlaying) { yield break; }
        m_isPlaying = true;
        if (onStart != null) onStart.Invoke();
        bool isPositive = true;

        while (true)
        {
            float t = Time.deltaTime / animationTime;

            if (isPositive)
            {
                progress += t;
                if (progress > 1.0f)
                {
                    isPositive = false;
                    progress -= t;
                }

            }
            else
            {

                progress -= t;
                if (progress < 0.0f)
                {
                    isPositive = true;
                    progress += t;
                }
            }

            yield return null;
        }
    }

#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1

#elif UNITY_5_2_0
    public override void ModifyMesh (Mesh mesh)
    {
        if (!IsActive())
            return;
        
        List<UIVertex> list = new List<UIVertex>();
        using (VertexHelper vertexHelper = new VertexHelper(mesh))
        {
            vertexHelper.GetUIVertexStream(list);
        }

        List<UIVertex> modifiedList4 = new List<UIVertex>();
        for (int i = 0; i < list.Count; i++)
        {
            int num = i % 6;
            if(num == 0 || num == 1 || num == 2 || num == 4)
            {
                modifiedList4.Add(list[i]);
            }
        }

        // calls the old ModifyVertices which was used on pre 5.2
        ModifyVertices(modifiedList4);

        List<UIVertex> modifiedList6 = new List<UIVertex>(list.Count);

        for (int i = 0; i < list.Count / 6; i++)
        {
            int i4 = i * 4;
            modifiedList6.Add(modifiedList4[i4]);
            modifiedList6.Add(modifiedList4[i4 + 1]);
            modifiedList6.Add(modifiedList4[i4 + 2]);
            modifiedList6.Add(modifiedList4[i4 + 2]);
            modifiedList6.Add(modifiedList4[i4 + 3]);
            modifiedList6.Add(modifiedList4[i4]);
        }

        using (VertexHelper vertexHelper2 = new VertexHelper())
        {
            vertexHelper2.AddUIVertexTriangleStream(modifiedList6);
            vertexHelper2.FillMesh(mesh);
        }
    }

// greater than 5.2.1
#else
    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
            return;

        List<UIVertex> list = new List<UIVertex>();
        vertexHelper.GetUIVertexStream(list);

        List<UIVertex> modifiedList4 = new List<UIVertex>();
        for (int i = 0; i < list.Count; i++)
        {
            int num = i % 6;
            if (num == 0 || num == 1 || num == 2 || num == 4)
            {
                modifiedList4.Add(list[i]);
            }
        }

        // calls the old ModifyVertices which was used on pre 5.2
        ModifyVertices(modifiedList4);

        List<UIVertex> modifiedList6 = new List<UIVertex>(list.Count);

        for (int i = 0; i < list.Count / 6; i++)
        {
            int i4 = i * 4;
            modifiedList6.Add(modifiedList4[i4]);
            modifiedList6.Add(modifiedList4[i4 + 1]);
            modifiedList6.Add(modifiedList4[i4 + 2]);
            modifiedList6.Add(modifiedList4[i4 + 2]);
            modifiedList6.Add(modifiedList4[i4 + 3]);
            modifiedList6.Add(modifiedList4[i4]);
        }

        vertexHelper.Clear();
        vertexHelper.AddUIVertexTriangleStream(modifiedList6);
    }
#endif

#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1
    public override void ModifyVertices(List<UIVertex> verts)
#else
    public void ModifyVertices(List<UIVertex> verts)
#endif
    {
        if (!IsActive())
            return;

        Modify(verts);
    }

    void Modify(List<UIVertex> verts)
    {
        characterNumber = verts.Count / 4;
        int currentCharacterNumber = 0;

        for (int i = 0; i < verts.Count; i++)
        {
            var maxGlyphSize = Vector2.zero;
            var text = "";
            var size = 12;


            if (i % 4 == 0)
            {
                currentCharacterNumber = i / 4;
                UIVertex uiVertex0 = verts[i];
                UIVertex uiVertex1 = verts[i + 1];
                UIVertex uiVertex2 = verts[i + 2];
                UIVertex uiVertex3 = verts[i + 3];

                if (usePosition)
                {
                    float temp = positionAnimationCurve.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, positionSeparation));
                    Vector3 offset = (positionTo - positionFrom) * temp + positionFrom;
                    uiVertex0.position += offset;
                    uiVertex1.position += offset;
                    uiVertex2.position += offset;
                    uiVertex3.position += offset;
                }

                if (useScale)
                {
                    if (scaleSyncXY)
                    {
                        float temp = scaleAnimationCurve.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, scaleSeparation));
                        float offset = (scaleTo - scaleFrom) * temp + scaleFrom;
                        float centerX = (uiVertex1.position.x - uiVertex3.position.x) * scalePivot.x + uiVertex3.position.x;
                        float centerY = (uiVertex1.position.y - uiVertex3.position.y) * scalePivot.y + uiVertex3.position.y;
                        Vector3 center = new Vector3(centerX, centerY, 0);
                        uiVertex0.position = (uiVertex0.position - center) * offset + center;
                        uiVertex1.position = (uiVertex1.position - center) * offset + center;
                        uiVertex2.position = (uiVertex2.position - center) * offset + center;
                        uiVertex3.position = (uiVertex3.position - center) * offset + center;

                    }
                    else
                    {

                        float temp = scaleAnimationCurve.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, scaleSeparation));
                        float offset = (scaleTo - scaleFrom) * temp + scaleFrom;
                        float centerX = (uiVertex1.position.x - uiVertex3.position.x) * scalePivot.x + uiVertex3.position.x;
                        float centerY = (uiVertex1.position.y - uiVertex3.position.y) * scalePivot.y + uiVertex3.position.y;
                        Vector3 center = new Vector3(centerX, centerY, 0);
                        uiVertex0.position = new Vector3(((uiVertex0.position - center) * offset + center).x, uiVertex0.position.y, uiVertex0.position.z);
                        uiVertex1.position = new Vector3(((uiVertex1.position - center) * offset + center).x, uiVertex1.position.y, uiVertex1.position.z);
                        uiVertex2.position = new Vector3(((uiVertex2.position - center) * offset + center).x, uiVertex2.position.y, uiVertex2.position.z);
                        uiVertex3.position = new Vector3(((uiVertex3.position - center) * offset + center).x, uiVertex3.position.y, uiVertex3.position.z);

                        temp = scaleAnimationCurveY.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, scaleSeparation));
                        offset = (scaleToY - scaleFromY) * temp + scaleFromY;
                        centerX = (uiVertex1.position.x - uiVertex3.position.x) * scalePivotY.x + uiVertex3.position.x;
                        centerY = (uiVertex1.position.y - uiVertex3.position.y) * scalePivotY.y + uiVertex3.position.y;
                        center = new Vector3(centerX, centerY, 0);
                        uiVertex0.position = new Vector3(uiVertex0.position.x, ((uiVertex0.position - center) * offset + center).y, uiVertex0.position.z);
                        uiVertex1.position = new Vector3(uiVertex1.position.x, ((uiVertex1.position - center) * offset + center).y, uiVertex1.position.z);
                        uiVertex2.position = new Vector3(uiVertex2.position.x, ((uiVertex2.position - center) * offset + center).y, uiVertex2.position.z);
                        uiVertex3.position = new Vector3(uiVertex3.position.x, ((uiVertex3.position - center) * offset + center).y, uiVertex3.position.z);
                    }
                }

                if (useRotation)
                {
                    float temp = rotationAnimationCurve.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, rotationSeparation));
                    float offset = (rotationTo - rotationFrom) * temp + rotationFrom;
                    float centerX = (uiVertex1.position.x - uiVertex3.position.x) * rotationPivot.x + uiVertex3.position.x;
                    float centerY = (uiVertex1.position.y - uiVertex3.position.y) * rotationPivot.y + uiVertex3.position.y;
                    Vector3 center = new Vector3(centerX, centerY, 0);
                    uiVertex0.position = Quaternion.AngleAxis(offset, Vector3.forward) * (uiVertex0.position - center) + center;
                    uiVertex1.position = Quaternion.AngleAxis(offset, Vector3.forward) * (uiVertex1.position - center) + center;
                    uiVertex2.position = Quaternion.AngleAxis(offset, Vector3.forward) * (uiVertex2.position - center) + center;
                    uiVertex3.position = Quaternion.AngleAxis(offset, Vector3.forward) * (uiVertex3.position - center) + center;
                }

                Color col = uiVertex0.color;

                foreach (var character in text)
                {
                    // 文字情報の取得をするが出来なかったら（本来は事前にリクエストしているので取得できるはず）
                    CharacterInfo info;
                    if (!Font.GetCharacterInfo(character, out info, size, style))
                    {
                        // そのまま次へ
                        continue;
                    }


                    // グリフの最大値を更新する
                    maxGlyphSize.x = Mathf.Max(maxGlyphSize.x, info.glyphWidth);
                    maxGlyphSize.y = Mathf.Max(maxGlyphSize.y, info.glyphHeight);
                }

                if (useColor)
                {
                    float temp = colorAnimationCurve.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, colorSeparation));
                    col = (colorTo - colorFrom) * temp + colorFrom;
                    uiVertex0.color = uiVertex1.color = uiVertex2.color = uiVertex3.color = col;
                }

                if (useAlpha)
                {
                    float temp = alphaAnimationCurve.Evaluate(SeparationRate(progress, currentCharacterNumber, characterNumber, alphaSeparation));
                    float a = (alphaTo - alphaFrom) * temp + alphaFrom;
                    col = new Color(col.r, col.g, col.b, col.a * a);
                    uiVertex0.color = uiVertex1.color = uiVertex2.color = uiVertex3.color = col;
                }

                verts[i] = uiVertex0;
                verts[i + 1] = uiVertex1;
                verts[i + 2] = uiVertex2;
                verts[i + 3] = uiVertex3;
            }
        }
    }

    static float SeparationRate(float progress, int currentCharacterNumber, int characterNumber, float separation)
    {
        return Mathf.Clamp01((progress - currentCharacterNumber * separation / characterNumber) / (separation / characterNumber + 1 - separation));
    }
}