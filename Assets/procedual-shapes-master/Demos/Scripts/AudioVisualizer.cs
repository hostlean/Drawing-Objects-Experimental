
using UnityEngine;


[RequireComponent(typeof(AudioSource))]

public class AudioVisualizer : MonoBehaviour {

   public float warmup = 3f;

   public float initialBufferDecrease = 0.0005f;
   public float bufferIncreaseMultiplier = 1.2f;

   public float circleAMaximum = 0.25f;
   public float circleBMaximum = 0.45f;
   public float circleCMultiplier = 50f;

   public ProceduralShapes.ProceduralCircle[] circles = new ProceduralShapes.ProceduralCircle[9];

   private float wait;
   private bool playing = false;
   
   private AudioSource audioSource;
   private float[] samples = new float[512];
   private float[] freqBands = new float[8];
   private float[] bandBuffers = new float[8];
   private float[] bufferDecreases = new float[8];

   private float[] freqBandMaxs = new float[8];
   private float[] audioBands = new float[8];
   private float[] audioBandBuffers = new float[8];

   void Start () {
      audioSource = GetComponent<AudioSource>();

      for (int i = 0; i < 9; i++) {
         circles[i].synchronise = 1f / 60f;
      }

      wait = warmup;
	}

   void Update () {
      if (wait > 0f) {
         wait -= Time.deltaTime;
      }
      else if (!playing) {
         audioSource.Play();
         playing = true;
      }
      else {
         GetSampleData();
         MakeFrequencyBands();
         MakeBandBuffers();
         MakeAudioBands();

         SetCircles();
      }
   }

   private void GetSampleData() {
      audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
   }
   private void MakeBandBuffers() {
      for (int i = 0; i < 8; i++) {
         if (freqBands[i] > bandBuffers[i]) {
            bandBuffers[i] = freqBands[i];
            bufferDecreases[i] = initialBufferDecrease;
         }
         else if (freqBands[i] < bandBuffers[i]) {
            bandBuffers[i] -= bufferDecreases[i];
            bufferDecreases[i] *= bufferIncreaseMultiplier;
         }
      }
   }
   private void MakeFrequencyBands() {

      /*
       *  22050 hz / 512 samples = 43 hz per sample
       * 
       *  Sub Bass       = 20 - 60 hz
       *  Bass           = 60 - 250 hz
       *  Low Midrange   = 250 - 500 hz
       *  Midrange       = 500 - 2000 hz
       *  Upper Midrange = 2000 - 4000 hz
       *  Presence       = 4000 - 6000 hz
       *  Brilliance     = 6000 - 20000 hz
       *
       *  0:   2 =    86 hz  |    0-1   =     0-86
       *  1:   4 =   172 hz  |    2-5   =    87-258
       *  2:   8 =   344 hz  |    6-13  =   259-602
       *  3:  16 =   688 hz  |   14-29  =   603-1290
       *  4:  32 =  1376 hz  |   30-61  =  1291-2666
       *  5:  64 =  2752 hz  |   62-125 =  2667-5418
       *  6: 129 =  5547 hz  |  126-254 =  5419-10965
       *  7: 257 = 11051 hz  |  255-511 = 10967-22006
       *  
       */

      int count = 0;
      for (int i = 0; i < 8; i++) {
         int sampleCount = (int)Mathf.Pow(2, i) * 2;
         if (i > 5)
            sampleCount++;
         float average = 0f;
         for (int n = 0; n < sampleCount; n++) {
            average += samples[count] * (count + 1);
            count++;
         }
         average /= count;
         freqBands[i] = average; 
      }
   }
   private void MakeAudioBands() {
      for (int i = 0; i < 8; i++) {
         if (freqBands[i] > freqBandMaxs[i])
            freqBandMaxs[i] = freqBands[i];
         audioBands[i] = (freqBands[i] / freqBandMaxs[i]);
         audioBandBuffers[i] = (bandBuffers[i] / freqBandMaxs[i]);
      }
   }

   private void SetCircles() {
      circles[0].border = circleAMaximum * audioBandBuffers[0];
      for (int i = 1; i < 8; i++) {
         circles[i].radius = circleBMaximum * audioBandBuffers[i];
      }

      circles[8].circleOffset = ProceduralShapes.ProcUtility.SpinFloat(circles[8].circleOffset, Time.deltaTime * circleCMultiplier, 0f, 360f, 0.1f);
   }
}
