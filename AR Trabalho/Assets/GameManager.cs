using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GameManager : MonoBehaviour
{
    public enum TipoAlvo {Real, Falso};

    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    private ARPlane plano;
    private GameObject cursor;
    [SerializeField] public GameObject[] alvos;
    public Canvas canvas;
    private Text msg;
    private Text placar;
    private GameObject Aim;

    //Controle do Jogo
    private int pontuacao;
    public int numeroAlvos = 10;
    private bool acabou = false;
    void Start()
    {
        msg = canvas.transform.Find(StringUtils.Childs.msg).GetComponent<Text>();
        placar = canvas.transform.Find(StringUtils.Childs.Pontuacao).GetComponent<Text>();
        Aim = canvas.transform.Find(StringUtils.Childs.Aim).gameObject;
        msg.text = "Mapeir o chao e toque na tela para comecar";
    }

    // Update is called once per frame
    void Update()
    {
        if (!acabou)
        {
            TratarPlanos();

            TratarMira();
        }
        
    }

    private void TratarPlanos()
    {
        if (plano != null)
        {
            return;
        }

        var posicaoTela = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));

        var raycastHits = new List<ARRaycastHit>();

        raycastManager.Raycast(posicaoTela, raycastHits, TrackableType.PlaneWithinBounds);

        if (raycastHits.Count > 0)
        {
            transform.position = raycastHits[0].pose.position;
            transform.rotation = raycastHits[0].pose.rotation;

            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ComecarJogo(raycastHits[0]);
            }
        }
    }

    
    private void ComecarJogo(ARRaycastHit raycastHit)
    {
        var idPlano = raycastHit.trackableId;
        plano = planeManager.GetPlane(idPlano);

        Aim.SetActive(false);
        planeManager.enabled = false;

        CriarAlvo();
    }

    private Vector3 ObterPontoAleatorio()
    {
        var x = Random.Range(-1f, 1f);
        var z = Random.Range(-1f, 1f);
        var y = Random.Range(.1f, 2f);

        var vetorAleatorio = new Vector3(x ,y, z);

        var posicaoAleatoria = plano.transform.TransformPoint(vetorAleatorio);

        return posicaoAleatoria;
    }

    private void CriarAlvo()
    {
        var pontoAleatorio = ObterPontoAleatorio();
        GameObject target;

        if (Random.Range(0, 101) < 25)
        { 
            target = GameObject.Instantiate(alvos[(int)TipoAlvo.Falso], pontoAleatorio, Quaternion.identity);
            Destroy(target, 3);
        }
        else
            target = GameObject.Instantiate(alvos[(int)TipoAlvo.Real], pontoAleatorio, Quaternion.identity);

        target.transform.LookAt(Camera.main.transform);
    }

    private void TratarMira()
    {
        RaycastHit hitInfo;

        var cam = Camera.main.transform;

        if(Physics.Raycast(cam.position, cam.forward, out hitInfo, 10000f))
        {
            if (hitInfo.transform.CompareTag("Alvo") && hitInfo.transform.CompareTag("AlvoFalso"))
                Aim.GetComponent<Image>().color = Color.red;
            else
                Aim.GetComponent<Image>().color = Color.white;


            if (hitInfo.transform.CompareTag("Alvo") && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Destroy(hitInfo.transform.gameObject);
                TratarAcerto();
            }

            if (hitInfo.transform.CompareTag("AlvoFalso") && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log("Se fudeu pegou o alvo falso");
                TratarAcertoAlvoFalso();
            }

        }
    }

    private void TratarAcertoAlvoFalso() 
    {
        acabou = true;
        msg.text = "Fim de jogo.\n\n Sua pontua��o � de " + pontuacao;
    }

    private void TratarAcerto()
    {
        pontuacao += 10;

        placar.text = pontuacao.ToString();

        numeroAlvos --;

        if (numeroAlvos > 0)
        {
            int numeroAlvos = Random.Range(1, 4);
            for (int i = 0; i < numeroAlvos; i++)
            {
                CriarAlvo();
            }
        }
        else
        {
            acabou = true;
            msg.text = "Fim de jogo.\n\n Sua pontua��o � de " + pontuacao;
        }
    }

    private void TratarFimJogo()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
