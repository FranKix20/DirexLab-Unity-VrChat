# DirexLab — Mundo Educativo VRChat en Unity

> Entorno de realidad virtual interactivo desarrollado en Unity para VRChat, orientado a la enseñanza de **principios de diseño arquitectónico y visual** mediante una sala inmersiva, paneles expositivos y un sistema de quiz interactivo.

---

## Tabla de contenidos

1. [Descripción general](#descripción-general)
2. [Tecnologías y requisitos](#tecnologías-y-requisitos)
3. [Estructura del proyecto](#estructura-del-proyecto)
4. [Escenas](#escenas)
5. [Scripts principales](#scripts-principales)
6. [Assets de terceros](#assets-de-terceros)
7. [Principios de diseño cubiertos](#principios-de-diseño-cubiertos)
8. [Configuración y uso](#configuración-y-uso)
9. [Flujo de trabajo Git](#flujo-de-trabajo-git)
10. [Contribuidores](#contribuidores)
11. [Licencia](#licencia)

---

## Descripción general

**DirexLab** es un mundo de VRChat construido en Unity que funciona como laboratorio educativo virtual. Los visitantes pueden recorrer una sala de diseño arquitectónico interior, observar paneles visuales sobre principios formales del diseño (simetría, equilibrio, fragmentación, etc.) y responder un quiz interactivo con retroalimentación inmediata de color y texto explicativo.

El proyecto fue desarrollado como parte de un curso universitario y combina modelado 3D propio (Blender → FBX), assets de terceros (mobiliario, skybox, texturas) y scripting con UdonSharp para la lógica interactiva dentro de VRChat.

---

## Tecnologías y requisitos

| Componente | Versión / Detalle |
|---|---|
| Unity Editor | 2022.3.22f1 (LTS) |
| VRChat SDK | SDK3 Worlds (via VPM) |
| UdonSharp | Última versión compatible con el SDK instalado |
| TextMesh Pro | 3.0.6 |
| USharpVideo | Incluido en Assets |
| Oculus XR Plugin | Incluido vía XR Settings |
| VPM Resolver | com.vrchat.core.vpm-resolver |
| IDE recomendado | Visual Studio / Rider / VS Code |

### Requisitos previos

- Unity Hub instalado con el editor **Unity 2022.3.22f1**.
- **VRChat Creator Companion (VCC)** para resolver dependencias VPM automáticamente.
- Cuenta de VRChat activa para publicar el mundo (Trust Rank: New User o superior).

---

## Estructura del proyecto

```
DirexLab-Unity-VrChat/
├── Assets/
│   ├── AllSkyFree/               # Skybox "Epic GloriousPink"
│   ├── Gogo Casual Pack/         # Pack de lámparas decorativas (FBX + prefabs)
│   ├── Ladymito/                 # Modelo 3D de gato animado (Free Cat)
│   ├── LowPolyLivingRoomPack/    # Muebles low-poly (sillón, sofá, mesa, etc.)
│   ├── Models/                   # Modelos propios y texturas de principios de diseño
│   │   ├── Modelo_Base-Sala-2P.fbx
│   │   ├── SalaRemodelada1.fbx
│   │   ├── Puff/                 # Modelo generado con Tripo3D
│   │   └── Materials/            # Materiales nombrados por principios de diseño
│   ├── Original Wood Textures/   # Texturas de madera (diffuse + normal map)
│   ├── Texturas de marmol/       # Texturas PBR de mármol (albedo, normal, AO)
│   ├── Scenes/
│   │   ├── VRCDefaultWorldScene.unity   # Escena base por defecto de VRChat
│   │   └── VRCDirexlab.unity            # Escena principal del mundo DirexLab
│   ├── Scripts/
│   │   └── QuizManager.cs        # Script principal del sistema de quiz
│   ├── Sprites/
│   │   ├── icon_correct.png
│   │   └── icon_incorrect.png
│   ├── TextMesh Pro/             # Assets TMP (fuentes, shaders, estilos)
│   ├── UdonSharp/                # Scripts utilitarios de UdonSharp
│   ├── USharpVideo/              # Reproductor de video sincronizado para VRChat
│   └── SerializedUdonPrograms/   # Programas Udon compilados (auto-generados)
├── Packages/
│   ├── manifest.json             # Dependencias del proyecto Unity
│   ├── vpm-manifest.json         # Dependencias VPM (VRChat Package Manager)
│   └── com.vrchat.core.vpm-resolver/
├── ProjectSettings/              # Configuración del proyecto Unity
├── ClientSimStorage/             # Datos de simulación local (ClientSim)
│   └── PlayerData/
│       └── PlayerData_1_VRCDirexlab.json
└── .gitignore
```

---

## Escenas

### `VRCDirexlab.unity` *(Escena principal)*

Esta es la escena activa del mundo. Contiene:

- El modelo de la sala remodelada (`SalaRemodelada1.fbx`) con materiales PBR de mármol y madera.
- Paneles visuales con imágenes de los principios de diseño, organizados como una galería expositiva.
- El sistema de quiz interactivo vinculado a `QuizManager.cs`.
- El gato decorativo animado (`Ladymito/Free_cat`).
- Mobiliario low-poly (sillón, sofá, mesa, alfombra) del pack `LowPolyLivingRoomPack`.
- Lámparas decorativas del pack `Gogo Casual Pack`.
- Un reproductor de video sincronizado (`USharpVideo`).
- Skybox rosado-celeste (`Epic_GloriousPink`).

### `VRCDefaultWorldScene.unity`

Escena de referencia por defecto provista por el SDK de VRChat. Se conserva como punto de partida y para pruebas con ClientSim. No es la escena que se sube al mundo.

---

## Scripts principales

### `QuizManager.cs`

Script UdonSharp ubicado en `Assets/Scripts/QuizManager.cs`. Gestiona la lógica completa del quiz interactivo dentro del mundo VRChat.

**Responsabilidades:**

- Mostrar una pregunta y hasta 4 opciones de respuesta (A, B, C, D) mediante botones UI y TextMesh Pro.
- Evaluar la respuesta seleccionada comparándola con `respuestaCorrecta`.
- Cambiar el color de los botones para dar retroalimentación visual (verde para correcto, rojo para incorrecto).
- Mostrar un panel de explicación (`panelExplicacion`) con texto descriptivo cuando la respuesta es correcta.
- Ocultar los botones temporalmente y reactivar el quiz automáticamente usando `SendCustomEventDelayedSeconds`.

**Campos configurables desde el Inspector de Unity:**

| Campo | Tipo | Descripción |
|---|---|---|
| `pregunta` | `string` | Texto de la pregunta a mostrar |
| `respuestas` | `string[]` | Arreglo con las 4 opciones de respuesta |
| `respuestaCorrecta` | `int` | Índice (0–3) de la opción correcta |
| `explicacion` | `string` | Texto que aparece al responder correctamente |
| `preguntaText` | `TMP_Text` | Referencia al componente de texto de la pregunta |
| `textoExplicacion` | `TMP_Text` | Referencia al componente de texto de la explicación |
| `panelExplicacion` | `GameObject` | Panel que se activa al acertar |
| `botones` | `Button[]` | Arreglo de botones UI de respuesta |
| `textosBotones` | `TMP_Text[]` | Textos de cada botón |
| `colorCorrecto` | `Color` | Color al acertar (por defecto: verde) |
| `colorIncorrecto` | `Color` | Color al fallar (por defecto: rojo) |
| `colorNormal` | `Color` | Color inicial de los botones (por defecto: blanco) |

**Flujo de la lógica:**

```
Start()
  └─► CargarPregunta()         ← resetea botones, limpia texto, oculta panel

RespuestaA/B/C/D()
  └─► Responder(index)
        ├─ Si ya respondió → return (anti-spam)
        ├─ Pinta todos los botones
        ├─ Si correcto → muestra panel + explicación → oculta botones → Resetear(60s)
        └─ Si incorrecto → Resetear(3s)

Resetear()
  └─► muestra botones → CargarPregunta()
```

**Nota sobre un bug conocido:** En la versión actual, al responder tanto correcta como incorrectamente, el bucle `for` aplica `colorIncorrecto` a todos los botones (la condición `if (i == respuestaCorrecta)` también asigna el color incorrecto). Esto es un bug identificado: el botón correcto debería recibir `colorCorrecto`.

---

### Scripts utilitarios de UdonSharp (`Assets/UdonSharp/UtilityScripts/`)

Estos scripts son provistos por el paquete de UdonSharp como utilidades listas para usar:

| Script | Función |
|---|---|
| `BoneFollower.cs` | Hace que un objeto siga un hueso del avatar del jugador |
| `InteractToggle.cs` | Activa/desactiva un `GameObject` al interactuar con él |
| `PlayerModSetter.cs` | Modifica parámetros del jugador (velocidad de caminata, salto, etc.) |
| `GlobalToggleObject.cs` | Toggle sincronizado para todos los jugadores en la instancia |
| `MasterToggleObject.cs` | Toggle controlado solo por el Master de la instancia |
| `TrackingDataFollower.cs` | Sigue datos de tracking de VR (cabeza, manos) |
| `WorldAudioSettings.cs` | Configura parámetros globales de audio del mundo |

---

### Scripts de USharpVideo (`Assets/USharpVideo/Scripts/`)

Reproductor de video sincronizado para VRChat. Los scripts clave son:

| Script | Función |
|---|---|
| `USharpVideoPlayer.cs` | Lógica principal del reproductor (carga, sincronización, controles) |
| `VideoPlayerManager.cs` | Gestión de múltiples reproductores y alternancia entre Unity Video Player y AVPro |
| `VideoControlHandler.cs` | Manejo de controles UI (play, pause, seek, volumen) |
| `SyncModeController.cs` | Control del modo de sincronización (propietario / todos) |
| `VolumeController.cs` | Control de volumen mediante slider |

---

## Assets de terceros

Todos los assets de terceros incluidos en el proyecto son de uso gratuito. A continuación, el detalle:

| Asset | Origen | Uso en el proyecto |
|---|---|---|
| **AllSkyFree** | Unity Asset Store (Free) | Skybox del mundo (`Epic_GloriousPink`) |
| **Gogo Casual Free Light Pack** | Unity Asset Store (Free) | Lámparas de techo, pared y mesa (FBX + prefabs) |
| **Free Low Poly Cat** (Ladymito) | Unity Asset Store (Free) | Gato decorativo animado |
| **Low Poly Living Room Pack** | Unity Asset Store (Free) | Mobiliario: sillón, sofá, mesa, alfombra, gamepad |
| **Original Wood Textures** | Asset Store / Recursos externos (Free) | Texturas de madera para pisos o mobiliario |
| **Streaked Marble / Stringy Marble / White Marble** | Recursos PBR externos (Free) | Texturas de mármol para paredes y pisos |
| **USharpVideo** | [GitHub - MerlinVR/USharpVideo](https://github.com/MerlinVR/USharpVideo) | Reproductor de video sincronizado para VRChat |
| **TextMesh Pro** | Unity (built-in package) | Renderizado de texto de alta calidad en los paneles UI |

---

## Principios de diseño cubiertos

El mundo expone visualmente los siguientes principios de diseño mediante paneles con imágenes y materiales etiquetados:

| Principio | Descripción breve |
|---|---|
| **Simetría** | Distribución equilibrada de elementos a partir de un eje central |
| **Asimetría** | Equilibrio visual sin espejo, usando pesos visuales diferentes |
| **Equilibrio** | Balance entre los elementos de una composición |
| **Fragmentación** | División de una forma en partes separadas pero relacionadas |
| **Inestabilidad** | Tensión visual que sugiere movimiento o desequilibrio |
| **Irregularidad** | Variación intencional que rompe patrones repetitivos |
| **Regularidad** | Repetición ordenada de elementos para crear ritmo |
| **Economía** | Uso mínimo de elementos para máxima comunicación |
| **Simplicidad** | Reducción a lo esencial, sin decoración superflua |
| **Unidad** | Cohesión visual entre todos los elementos de la composición |

Cada principio cuenta con al menos una imagen de referencia (PNG) y un material asociado en `Assets/Models/Materials/`.

---

## Configuración y uso

### 1. Clonar el repositorio

```bash
git clone https://github.com/FranKix20/DirexLab-Unity-VrChat.git
```

### 2. Abrir con VRChat Creator Companion (recomendado)

1. Abre el **VRChat Creator Companion (VCC)**.
2. Selecciona "Add Existing Project" y apunta a la carpeta clonada.
3. El VCC detectará automáticamente las dependencias VPM y te pedirá resolverlas.
4. Haz clic en "Resolve" para que se instalen el SDK de VRChat y UdonSharp.

### 3. Abrir el proyecto en Unity

1. Desde el VCC, haz clic en "Open Project" o ábrelo manualmente desde Unity Hub con **Unity 2022.3.22f1**.
2. Espera a que Unity compile todos los scripts y assets.
3. Si aparece el **VPM Resolver** en la ventana de VRChat SDK, haz clic en "Resolve" nuevamente.

### 4. Abrir la escena principal

Navega a `Assets/Scenes/` y abre `VRCDirexlab.unity`.

### 5. Probar localmente con ClientSim

1. Ve a `VRChat SDK → Show Control Panel`.
2. Haz clic en el botón **Play** de Unity o usa ClientSim para simular un jugador.
3. Los datos de simulación se guardan en `ClientSimStorage/PlayerData/`.

### 6. Publicar el mundo en VRChat

1. Inicia sesión en el panel del SDK (`VRChat SDK → Show Control Panel → Authentication`).
2. Configura el nombre, descripción, capacidad y tags del mundo.
3. Haz clic en **Build & Publish** para compilar y subir el mundo.

---

## Flujo de trabajo Git

El proyecto sigue un flujo de trabajo con ramas por funcionalidad:

- `main` — Rama estable con las últimas funcionalidades integradas.
- Ramas de feature — Se crean desde `main` para cada avance individual (ej. `Avance_Jhon_FeedbackPanel`, etc.).

### Convención de commits

```
[Tipo]: Descripción breve

Tipos sugeridos: feat, fix, refactor, assets, docs, chore
```

Ejemplos:
```
feat: agregar sistema de quiz con retroalimentación de color
assets: importar LowPolyLivingRoomPack con prefabs
fix: corregir bug de color en botón correcto del QuizManager
docs: actualizar README con descripción de scripts
```

---

## Contribuidores

| Nombre | Rol |
|---|---|
| Bastian Encina | Líder |
| Jhon Santa Cruz ([@Acuario22](https://github.com/Acuario22)) | Analista |
| Benjamin Muñoz ([@bmunozs](https://github.com/bmunozs)) | Investigador |
| Franco Quintuman ([@FranKix20](https://github.com/FranKix20)) | Desarrollador |

---

## Licencia

Este proyecto está licenciado bajo la **MIT License**. Consulta el archivo [LICENSE](./LICENSE) para más detalles.

Los assets de terceros incluidos en este repositorio conservan sus propias licencias originales. Revisa los archivos de licencia dentro de cada carpeta de asset correspondiente antes de redistribuir.

---

> Proyecto UTEM — GPI · DirexLab · Unity 2022 + VRChat SDK3 + UdonSharp
