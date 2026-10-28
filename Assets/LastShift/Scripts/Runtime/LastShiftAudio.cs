using UnityEngine;
namespace LastShift {
/// <summary>Optional synthesized placeholder audio; no external sound files.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class LastShiftAudio:MonoBehaviour {
 private AudioSource sfx;
 private AudioClip pickup,locked,danger,win;
 private void Awake(){
  sfx=GetComponent<AudioSource>();sfx.spatialBlend=0;sfx.playOnAwake=false;
  pickup=Tone("Fuse",490,.15f);
  locked=Tone("Locked",160,.18f);
  danger=Tone("Danger",78,.75f);
  win=Tone("Victory",620,.5f);
 }
 public void PlayPickup()=>sfx.PlayOneShot(pickup);
 public void PlayDenied()=>sfx.PlayOneShot(locked);
 public void PlayDanger()=>sfx.PlayOneShot(danger);
 public void PlayVictory()=>sfx.PlayOneShot(win);
 private static AudioClip Tone(string name,float freq,float seconds){
  const int sampleRate=22050;
  int n=Mathf.CeilToInt(seconds*sampleRate);var audio=new float[n];
  for(int i=0;i<n;i++){
   float time=i/(float)sampleRate;
   float envelope=Mathf.Min(1f,Mathf.Min(time*14f,(seconds-time)*14f));
   audio[i]=Mathf.Sin(2*Mathf.PI*freq*time)*.12f*envelope;
  }
  var clip=AudioClip.Create(name,n,1,sampleRate,false);
  clip.SetData(audio,0);return clip;
 }
}
}