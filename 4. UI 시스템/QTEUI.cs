using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class QTEUI : MonoBehaviour
{
    public RectTransform needle;
    public float rotateSpeed = 90f;
    public KeyCode inputKey = KeyCode.Space;
    public Image successArc;
    public float minAngle;
    public float maxAngle;

    private float currentAngle = 0f;
    private bool isRunning = false;
    private bool goingBack = false; // 회전 방향
    private bool wasSuccess = false;
    
    private Action<bool> resultCallback;

    public void ShowQTEUI()
    {
        ShowQTEUI(null);
    }
    
    public void ShowQTEUI(Action<bool> callback)
    {
        currentAngle = 0f;
        needle.localEulerAngles = Vector3.zero;
        isRunning = true;
        goingBack = false;
        wasSuccess = false;
        resultCallback = callback;
        ShowSuccessArc();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isRunning) return;
        
        float delta = rotateSpeed * Time.unscaledDeltaTime;
        // currentAngle += rotateSpeed * Time.unscaledDeltaTime;

        if (!goingBack)
        {
            currentAngle += delta;
            if (currentAngle >= 180f)
            {
                currentAngle = 180f;
                goingBack = true; // 되돌아가기 시작
            }
        }
        else
        {
            currentAngle -= delta;
            if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                isRunning = false;
                gameObject.SetActive(false);

                if (!wasSuccess)
                {
                    InvokeResult(false);
                }
                return;
            }
        }
        
        needle.localEulerAngles = new Vector3(0, 0, -currentAngle);


        if (Input.GetKeyDown(inputKey))
        {
            wasSuccess = currentAngle >= minAngle && currentAngle <= maxAngle;
            isRunning = false;
            // gameObject.SetActive(false);
            InvokeResult(wasSuccess);
            // if (currentAngle >= minAngle && currentAngle <= maxAngle)
            // {
            //     wasSuccess = true;
            //     isRunning = false;
            //     gameObject.SetActive(false);
            //     PossessionQTESystem.Instance.HandleQTEResult(true);
            // }
            // else
            // {
            //     wasSuccess = false;
            //     isRunning = false;
            //     gameObject.SetActive(false);
            //     PossessionQTESystem.Instance.HandleQTEResult(false);
            //
            //
            // }
            // isRunning = false;
            // gameObject.SetActive(false);
            // bool success = (currentAngle >= minAngle && currentAngle <= maxAngle);
            // PossessionQTESystem.Instance.HandleQTEResult(success);
        }
    }

    private void InvokeResult(bool result)
    {
        if(resultCallback != null)
            resultCallback.Invoke(result);
        else
            PossessionQTESystem.Instance.HandleQTEResult(result);
        
        gameObject.SetActive(false);
    }

    void ShowSuccessArc()
    {
        minAngle = UnityEngine.Random.Range(20, 150);
        maxAngle = minAngle + 30;
        float fill = (maxAngle - minAngle) / 360f;
        successArc.fillAmount = fill;
        successArc.rectTransform.localEulerAngles = new Vector3(0, 0, -minAngle);
    }
}
