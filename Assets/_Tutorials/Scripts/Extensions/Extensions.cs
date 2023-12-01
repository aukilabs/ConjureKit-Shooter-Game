using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public static class Extensions
{
    public static string TagString(this string msg)
    {
        return $"CONJURE.KIT.SHOOTER: {msg}";
    }
    
    public static byte[] ToJsonByteArray(this object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
    { 
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    }));
    public static T FromJsonByteArray<T>(this byte[] byteArray) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));

    public static byte[] ToByteArray(this string str) => Encoding.UTF8.GetBytes(str);

    public static string ParseString(this byte[] data) => Encoding.UTF8.GetString(data);
    
    public static void LerpFloat(this MonoBehaviour mb,float start, float end, float duration, Action<float> onProgress, Action onComplete = null)
    {
        float val;
        var timer = duration;

        IEnumerator Lerp()
        {
            while (timer > 0f)
            {
                timer -= Time.deltaTime;

                val = Mathf.Lerp(start, end, 1f - timer / duration);
            
                onProgress?.Invoke(val);

                yield return null;
            }
        
            onProgress?.Invoke(end);
            onComplete?.Invoke();
        }

        mb.StartCoroutine(Lerp());
    }

    public static void DelayAction(this MonoBehaviour mb, Action action, float delay)
    {
        mb.StartCoroutine(DelayActionCoroutine(action, delay));
        
        IEnumerator DelayActionCoroutine(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            action?.Invoke();
        }
    }

    public static void PlayRandomPitch(this AudioSource source, AudioClip clip, float varRate)
    {
        if (source == null) return;
        
        source.pitch = UnityEngine.Random.Range(1f - varRate, 1f + varRate);
        source.clip = clip;
        source.Play();
    }
}
