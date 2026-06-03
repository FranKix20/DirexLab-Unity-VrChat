/*
 * ============================================================
 *  DirexLab VRChat — QuizFeedbackPanel.cs
 *  Autor: Franco
 *  Descripción: Controla el panel de feedback World Space Canvas
 *               que muestra al usuario si su respuesta es
 *               correcta o incorrecta en el quiz interactivo.
 * ============================================================
 *
 *  SETUP EN UNITY (pasos obligatorios):
 *  1. Crea un GameObject vacío llamado "FeedbackPanel" en la escena.
 *  2. Añade un Canvas (World Space) como hijo de FeedbackPanel.
 *     - Canvas Scaler → World Space → Reference Pixels Per Unit = 100
 *     - Ancho sugerido: 1.2 unidades | Alto: 0.9 unidades
 *     - Posición: frente al área de preguntas, ~1.5m de altura
 *  3. Dentro del Canvas crea la siguiente jerarquía:
 *
 *     Canvas (World Space)
 *     └── PanelRoot [Image - color de fondo]
 *         ├── PanelBorder [Image - borde decorativo, opcional]
 *         ├── IconoResultado [Image - icono ✓ o ✗]
 *         ├── TituloTexto [TextMeshPro] → "¡Correcto!" / "Incorrecto"
 *         ├── MensajeTexto [TextMeshPro] → explicación breve
 *         ├── PuntajeTexto [TextMeshPro] → puntos acumulados
 *         └── BotonSiguiente [Button + TextMeshPro] → "Siguiente"
 *
 *  4. Arrastra este script como componente de "FeedbackPanel".
 *  5. Asigna todas las referencias en el Inspector.
 *  6. Llama a ShowFeedback(bool esCorrecta, string explicacion, int puntaje)
 *     desde tu script de preguntas (QuizManager, etc.).
 * ============================================================
 */

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class QuizFeedbackPanel : UdonSharpBehaviour
{
    // ────────────────────────────────────────────────────────
    //  Referencias de UI (asignar en Inspector)
    // ────────────────────────────────────────────────────────

    [Header("Contenedor del panel")]
    [Tooltip("Objeto raíz del Canvas World Space. Se activa/desactiva.")]
    public GameObject panelRoot;

    [Header("Fondo del panel")]
    [Tooltip("Imagen de fondo — cambia de color según resultado.")]
    public Image fondoPanel;

    [Header("Icono de resultado")]
    [Tooltip("Imagen que muestra checkmark (correcto) o X (incorrecto).")]
    public Image iconoResultado;
    [Tooltip("Sprite para respuesta CORRECTA (checkmark verde).")]
    public Sprite spriteCorrecta;
    [Tooltip("Sprite para respuesta INCORRECTA (X roja).")]
    public Sprite spriteIncorrecta;

    [Header("Textos")]
    [Tooltip("Texto grande: '¡Correcto!' o 'Incorrecto'.")]
    public TextMeshProUGUI tituloTexto;
    [Tooltip("Texto secundario con explicación breve de la respuesta.")]
    public TextMeshProUGUI mensajeTexto;
    [Tooltip("Texto que muestra los puntos obtenidos y el total.")]
    public TextMeshProUGUI puntajeTexto;

    [Header("Botón Siguiente")]
    [Tooltip("Botón para avanzar a la siguiente pregunta.")]
    public Button botonSiguiente;

    [Header("Referencia al QuizManager")]
    [Tooltip("Script que gestiona las preguntas. Se le notifica al presionar Siguiente.")]
    public UdonSharpBehaviour quizManager;

    // ────────────────────────────────────────────────────────
    //  Colores del panel (ajustables en Inspector)
    // ────────────────────────────────────────────────────────

    [Header("Colores — Respuesta correcta")]
    public Color colorFondoCorrecto   = new Color(0.082f, 0.627f, 0.459f, 0.96f); // verde esmeralda
    public Color colorTituloCorrecto  = Color.white;

    [Header("Colores — Respuesta incorrecta")]
    public Color colorFondoIncorrecto  = new Color(0.749f, 0.216f, 0.216f, 0.96f); // rojo
    public Color colorTituloIncorrecto = Color.white;

    // ────────────────────────────────────────────────────────
    //  Animación (tiempos en segundos)
    // ────────────────────────────────────────────────────────

    [Header("Animación")]
    [Tooltip("Duración del fade-in y scale-in al aparecer el panel.")]
    [Range(0.1f, 0.8f)] public float duracionEntrada = 0.3f;
    [Tooltip("Duración del fade-out al cerrar el panel.")]
    [Range(0.1f, 0.8f)] public float duracionSalida  = 0.25f;
    [Tooltip("Segundos de espera antes de cerrar automáticamente (0 = no auto-cierre).")]
    [Range(0f, 10f)]    public float tiempoAutoCierre = 0f;

    // ────────────────────────────────────────────────────────
    //  Estado interno
    // ────────────────────────────────────────────────────────

    private bool   _animando      = false;
    private float  _timerAnim     = 0f;
    private float  _timerAutoCierre = 0f;
    private bool   _esperandoCierre = false;

    // Fases de animación: 0=idle, 1=entrando, 2=visible, 3=saliendo
    private int    _faseAnim = 0;

    private CanvasGroup _canvasGroup;

    // ────────────────────────────────────────────────────────
    //  Inicialización
    // ────────────────────────────────────────────────────────

    void Start()
    {
    _canvasGroup = panelRoot.GetComponent<CanvasGroup>();
    
    panelRoot.SetActive(false);
    _canvasGroup.alpha = 0f;
    panelRoot.transform.localScale = Vector3.zero;
    }

    // ────────────────────────────────────────────────────────
    //  API pública: mostrar el feedback
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra el panel de feedback con animación.
    /// Llamar desde QuizManager al evaluar la respuesta.
    /// </summary>
    /// <param name="esCorrecta">true = verde ✓ | false = rojo ✗</param>
    /// <param name="explicacion">Texto corto explicando la respuesta (puede ser "").</param>
    /// <param name="puntosObtenidos">Puntos ganados en esta pregunta.</param>
    /// <param name="puntajeTotal">Puntos acumulados hasta ahora.</param>
    public void ShowFeedback(bool esCorrecta, string explicacion, int puntosObtenidos, int puntajeTotal)
    {
        if (_animando) return; // evitar llamadas superpuestas

        // ── Configurar contenido ──────────────────────────
        if (esCorrecta)
        {
            fondoPanel.color      = colorFondoCorrecto;
            tituloTexto.text      = "¡Correcto!";
            tituloTexto.color     = colorTituloCorrecto;
            iconoResultado.sprite = spriteCorrecta;
            iconoResultado.color  = Color.white;
        }
        else
        {
            fondoPanel.color      = colorFondoIncorrecto;
            tituloTexto.text      = "Incorrecto";
            tituloTexto.color     = colorTituloIncorrecto;
            iconoResultado.sprite = spriteIncorrecta;
            iconoResultado.color  = Color.white;
        }

        mensajeTexto.text = (explicacion != "") ? explicacion : " ";

        if (esCorrecta && puntosObtenidos > 0)
            puntajeTexto.text = "+" + puntosObtenidos.ToString() + " pts   |   Total: " + puntajeTotal.ToString();
        else
            puntajeTexto.text = "Total: " + puntajeTotal.ToString() + " pts";

        // ── Iniciar animación de entrada ──────────────────
        panelRoot.SetActive(true);
        panelRoot.transform.localScale = Vector3.zero;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable  = false;
        _canvasGroup.blocksRaycasts = false;

        _faseAnim   = 1;        // fase: entrando
        _timerAnim  = 0f;
        _animando   = true;
        _esperandoCierre = false;
    }

    // ────────────────────────────────────────────────────────
    //  Cierre manual (desde botón Siguiente)
    // ────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por el botón "Siguiente" del panel.
    /// </summary>
    public void OnBotonSiguientePressed()
    {
        if (_faseAnim == 2) // solo si el panel está visible
            IniciarSalida();
    }

    // ────────────────────────────────────────────────────────
    //  Loop de animación (Update manual sin Coroutines — Udon)
    // ────────────────────────────────────────────────────────

    void Update()
    {
        if (!_animando && _faseAnim == 0) return;

        _timerAnim += Time.deltaTime;

        switch (_faseAnim)
        {
            // ── Fase 1: Fade + scale in ────────────────────
            case 1:
            {
                float t = Mathf.Clamp01(_timerAnim / duracionEntrada);
                float ease = EaseOutBack(t);

                _canvasGroup.alpha = t; // fade in lineal
                panelRoot.transform.localScale = Vector3.one * ease;

                if (t >= 1f)
                {
                    _canvasGroup.alpha = 1f;
                    panelRoot.transform.localScale = Vector3.one;
                    _canvasGroup.interactable   = true;
                    _canvasGroup.blocksRaycasts = true;
                    _faseAnim  = 2;    // fase: visible
                    _timerAnim = 0f;
                    _animando  = false;

                    // ¿auto-cierre activo?
                    if (tiempoAutoCierre > 0f)
                    {
                        _esperandoCierre = true;
                        _timerAutoCierre = 0f;
                    }
                }
                break;
            }

            // ── Fase 2: Visible — esperar auto-cierre ─────
            case 2:
            {
                if (_esperandoCierre)
                {
                    _timerAutoCierre += Time.deltaTime;
                    if (_timerAutoCierre >= tiempoAutoCierre)
                    {
                        _esperandoCierre = false;
                        IniciarSalida();
                    }
                }
                break;
            }

            // ── Fase 3: Fade + scale out ──────────────────
            case 3:
            {
                float t = Mathf.Clamp01(_timerAnim / duracionSalida);
                float inv = 1f - EaseInBack(t);

                _canvasGroup.alpha = 1f - t;
                panelRoot.transform.localScale = Vector3.one * inv;

                if (t >= 1f)
                {
                    panelRoot.SetActive(false);
                    _faseAnim  = 0;
                    _timerAnim = 0f;
                    _animando  = false;

                    // Notificar al QuizManager
                    if (quizManager != null)
                        quizManager.SendCustomEvent("OnFeedbackCerrado");
                }
                break;
            }
        }
    }

    // ────────────────────────────────────────────────────────
    //  Helpers internos
    // ────────────────────────────────────────────────────────

    private void IniciarSalida()
    {
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
        _faseAnim  = 3;
        _timerAnim = 0f;
        _animando  = true;
    }

    // Ease out back: rebote suave al aparecer
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Ease in back: se "encoge" antes de desaparecer
    private float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}

/*
 * ============================================================
 *  CÓMO LLAMARLO DESDE QuizManager (ejemplo)
 * ============================================================
 *
 *  // En tu script QuizManager.cs (UdonSharpBehaviour):
 *
 *  public QuizFeedbackPanel feedbackPanel;
 *  private int puntajeTotal = 0;
 *
 *  public void EvaluarRespuesta(int indiceSeleccionado)
 *  {
 *      bool esCorrecta = (indiceSeleccionado == respuestaCorrectaActual);
 *      int puntosGanados = esCorrecta ? 10 : 0;
 *      puntajeTotal += puntosGanados;
 *
 *      string explicacion = esCorrecta
 *          ? "¡Muy bien! Esa era la opción correcta."
 *          : "La respuesta correcta era: " + opciones[respuestaCorrectaActual];
 *
 *      feedbackPanel.ShowFeedback(esCorrecta, explicacion, puntosGanados, puntajeTotal);
 *  }
 *
 *  // Este método se llama automáticamente cuando el panel se cierra:
 *  public void OnFeedbackCerrado()
 *  {
 *      CargarSiguientePregunta();
 *  }
 *
 * ============================================================
 */
