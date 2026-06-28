
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : UdonSharpBehaviour
{
    [Header("Pregunta")]
    public string pregunta;
    public string[] respuestas;
    public int respuestaCorrecta;
    public string explicacion;

    [Header("UI")]
    public TMP_Text preguntaText;
    public TMP_Text textoExplicacion;

    public TMP_Text TextoInferior;

    public GameObject panelExplicacion;

    public Button[] botones;
    public TMP_Text[] textosBotones;

    [Header("Colores")]
    public Color colorIncorrecto = Color.red;
    public Color colorNormal = Color.white;

    private bool yaRespondio = false;

    void Start()
    {
        if (preguntaText == null) Debug.LogError("Falta preguntaText");
        if (textoExplicacion == null) Debug.LogError("Falta textoExplicacion");
        if (panelExplicacion == null) Debug.LogError("Falta panelExplicacion");

        CargarPregunta();
    }

    public void CargarPregunta()
    {
        yaRespondio = false;

        // Mostrar pregunta
        preguntaText.text = pregunta;

        // Mostrar respuestas
        for (int i = 0; i < textosBotones.Length; i++)
        {
            textosBotones[i].text = respuestas[i];

            Image img = botones[i].GetComponent<Image>();
            img.color = colorNormal;

            botones[i].interactable = true;
        }

        // Limpiar explicación y texto inferior
        textoExplicacion.text = "";

        TextoInferior.text = "";

        // Ocultar panel
        panelExplicacion.SetActive(false);
    }

    // FUNCIÓN PRINCIPAL
    public void Responder(int index)
    {
        if (yaRespondio) return;

        yaRespondio = true;

        for (int i = 0; i < botones.Length; i++)
        {
            Image img = botones[i].GetComponent<Image>();

            if (i == respuestaCorrecta)
                img.color = colorIncorrecto;
            else
                img.color = colorIncorrecto;

            botones[i].interactable = false;
        }

        if (index == respuestaCorrecta){
            preguntaText.text = "¡Respuesta Correcta!";
            textoExplicacion.text = explicacion;
            panelExplicacion.SetActive(true);
            foreach (Button b in botones)
                {
                 b.gameObject.SetActive(false);
                }
            SendCustomEventDelayedSeconds(nameof(Resetear), 60f);        
        }
        else{
            SendCustomEventDelayedSeconds(nameof(Resetear), 3f);
            TextoInferior.text = "Respuesta Incorrecta";
            }
        
    }

    // 🔹 FUNCIONES PARA BOTONES (UI / Udon)
    public void RespuestaA()
    {
        Responder(0);
    }

    public void RespuestaB()
    {
       
        Responder(1);
    }

    public void RespuestaC()
    {
        Responder(2);
    }

    public void RespuestaD()
    {
        Responder(3);
    }

    public void Resetear()
    {
        // Ocultar panel
        panelExplicacion.SetActive(false);

        // 🔥 VOLVER A MOSTRAR BOTONES
        foreach (Button b in botones)
        {
            b.gameObject.SetActive(true);
            b.interactable = true;
        }

        CargarPregunta();
    }
}   