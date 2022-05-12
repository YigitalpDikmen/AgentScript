using System.Collections; //Standard kutuphane
using System.Collections.Generic; //Standard kutuphane
using UnityEngine; //Standard kutuphane

using Unity.MLAgents; // Ozel Kutuphane
using Unity.MLAgents.Sensors; // Ozel Kutuphane
using Unity.MLAgents.Actuators; // Ozel Kutuphane

public class RollerAgent : Agent // RollerAgent sınıfımızı Agent sınıfından inherit ediyoruz (Agent sınıfının fonksiyonlarını kullanabilmek için)
{
    Rigidbody rBody; // Rigidbody sınıfının rBody nesnesi - Ajanımızın hız bilgisini alabilmek için oluşturduğumuz bir değişken.
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>(); //Ajanımızın hızını - velocity kontrol edebilmek için gerekli olan rigidbody referansı (Bu scripti aynı gameobject'e atıyoruz ki bilgiyi alabilelim)
    }

    public Transform Target; // Transform sınıfının public erişimli Target nesnesi - Unity sahnesindeki her obje transform sınıfı ile birlikte gelir ve transform sınıfı objeden kaldırılamaz.
    public override void OnEpisodeBegin() // Ajan problemi çözdüğünde veya çözemediğinde bölüm - episode biter. Yeni bölümü başlatıp çevreyi sıfırlayacak fonksiyon.
    {
       // If the Agent fell, zero its momentum - Ajan platformdan düştüğü zaman momentumunu (Hız x Kütle) sıfırlamak için yazılan if bloğu
        if (this.transform.localPosition.y < 0) // Ajanın Y eksenindeki başlangıç pozisyonunu 0.5 olarak atamıştık ve üzerinde durduğu platformu da Y'de 0 olarak atamıştık, dolayısıyla ajan düşerse Y eksenindeki konum bilgisi 0'ın altında (-) değer olarak gözükecek. Ve düştüğünü anlayacağız.
        {
            this.rBody.angularVelocity = Vector3.zero; // Vector3 unity'deki bir struct yapısıdır. 3 boyutlu vektör bilgilerini tutar. Vector3.zero --> Vector3(0, 0, 0) kısayoludur. Rigidbody'mizin x,y ve z yönlerindeki açısal hızını sıfırlamış olduk.  
            this.rBody.velocity = Vector3.zero; // Rigidbody'mizin x,y ve z yönlerindeki normal hızını sıfırlamış olduk.  
            this.transform.localPosition = new Vector3( 0, 0.5f, 0); // GameObjectimizi yani küre - sphere ajanımızı başlangıçtaki konumuna geri döndürmüş olduk.
        }

        // Move the target to a new spot - Hedefi her seferinde yeni bir noktaya konumlandırmamız gerekir.
        Target.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.5f,
                                           Random.value * 8 - 4); // Y eksenini sabit bırakıp X ve Z ekseninin konumunu random sayılarla değiştiriyoruz. Başka bir formül de yazılabilir. 
    }

public override void CollectObservations(VectorSensor sensor) //Ajanın başarılı olması için probleme analitik çözüm bulabilecek doğru bilgileri alması ve doğru gözlemi yapması gerekiyor. Bu problem için Ajana 8 değer iletiliyor. Ve Ajan da bu bilgileri neural networke "feature vector" olarak iletiyor.
{
    // Target and Agent positions
    sensor.AddObservation(Target.localPosition); // Hedefin pozisyonu (x,y,z) cinsinden 3 değer
    sensor.AddObservation(this.transform.localPosition); // Ajanın kendi pozisyonu (x,y,z) cinsinden 3 değer

    // Agent velocity
    sensor.AddObservation(rBody.velocity.x); // Ajanın x eksenindeki hızı 1 değer
    sensor.AddObservation(rBody.velocity.z); // Ajanın z eksenindeki hızı 1 değer
}        

public float forceMultiplier = 10; // ?? Burdan sonrası yazılacak. 
public override void OnActionReceived(ActionBuffers actionBuffers) // Yazılacak.
{
    // Actions, size = 2
    Vector3 controlSignal = Vector3.zero;
    controlSignal.x = actionBuffers.ContinuousActions[0];
    controlSignal.z = actionBuffers.ContinuousActions[1];
    rBody.AddForce(controlSignal * forceMultiplier);

    // Rewards
    float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

    // Reached target
    if (distanceToTarget < 1.42f)
    {
        SetReward(1.0f);
        EndEpisode();
    }

    // Fell off platform
    else if (this.transform.localPosition.y < 0)
    {
        EndEpisode();
    }
}

}
