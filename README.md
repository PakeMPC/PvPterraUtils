# ESPAÑOL

> **NOTA: ES NECESARIO TENER SSC (Server-side Character) ACTIVADO PARA QUE EL PLUGIN FUNCIONE CORRECTAMENTE**

PvPterraUtils es un sistema avanzado de gestión de PvP, arenas y control estricto de inventarios para servidores de Terraria (TShock). Diseñado para crear experiencias competitivas justas y equilibradas.

## ✨ Características Principales

* **⚔️ Arenas Dinámicas (FFA y Equipos):** Soporte para modos "Todos contra Todos" y batallas por equipos (Red, Blue, Green, Yellow, Pink).
* **🛡️ Gestión de Regiones PvP:** Te permite designar regiones con diferentes configuraciones y características de PvP (configurables en el archivo de configuración .json).
* **💰 Economía Integrada:** Cobra tarifas de entrada por entrar a regiones competitivas y otorga recompensas configurables por asesinato.
* **🔥 Rachas de Asesinatos (Killstreaks):** Anuncios globales en el chat para rachas destacadas (Triple, Penta, Unstoppable, Godlike).
* **👀 Modo Espectador Inteligente:** Puedes espectar cuando estás esperando a que acabe una batalla para entrar tú, o por decisión propia al usar /pvpteam spectator.
* **🔄 Reingreso Táctico (`AllowBackOnDeath`):** Opción configurable para permitir que los jugadores reaparezcan y vuelvan directamente a la batalla en curso en combates por equipos.
* **🌍 Soporte Multi-Idioma (i18n):** Traducción nativa y automática de todos los mensajes, menús y logs en **Español (es)**, **Inglés (en)** y **Portugués (pt)**, con soporte para expandir a más idiomas futuramente.
* **📦 Inventarios personalizados:** Permite definir cantidades exactas de munición o consumibles para la arena, cuando el jugador entre se cambiará su inventario a uno que hayas configurado.
* **🏆 Ranking:** Sistema de ranking global y por arena basado en muertes.


## 📜 Comandos del Jugador

| Comando | Parámetros | Permiso | Descripción |
| :--- | :--- | :--- | :--- |
| `/pvp info` | Ninguno | `pvpterra.user` | Muestra información de la zona actual (modo, recompensa, tiempo restante de combate). |
| `/pvpranking` | `[región]` | `pvpterra.ranking` | Muestra el Top 10 de jugadores con más puntos (Global si no se define una región, o por región si es definida). |
| `/pvpteam` | `<color(red, blue, pink, etc.)/none/spectator>`| `pvpterra.team` | Establece la preferencia de equipo antes de entrar a una arena. |


## 🛠️ Comandos de Administrador

*Todos estos comandos requieren el permiso:* `pvpterra.admin`

| Comando | Parámetros | Descripción |
| :--- | :--- | :--- |
|  `/pvpactive` | `[t/f] [modo] [recompensa] [--forceteam / --ignoreteam]` | Activa/Desactiva el PvP Global forzado en todo el mapa. |
| `/pvpregion assign` | `[nombre] [modo] [recompensa]` | Convierte una región de TShock existente en una Arena PvP. |
| `/pvpregion define` | `[nombre] [modo] [recompensa]` | Crea una nueva región de TShock y la define como Arena PvP. |
| `/pvpregion redefine`| `[nombre] [modo] [recompensa]` | Actualiza las reglas (modo/recompensa) de una arena existente. |
| `/pvpregion remove` | `[nombre]` | Quita las propiedades PvP de una región (mantiene la región física). |
| `/pvpregion delete` | `[nombre]` | Destruye completamente la región del servidor y del plugin. |
| `/pvpregion set` | `[1/2]` | Herramienta de selección para crear regiones con `/pvpregion define`. |

> **Flags de PvP Global (`/pvpactive`):**
> * `--forceteam`: Ignora la preferencia de los jugadores (`/pvpteam`) y los fuerza a dividirse equitativamente en los equipos que exija el modo (ej. si es 2vs2, forzará a mitades iguales de Rojos y Azules).
> * `--ignoreteam`: Permite que los jugadores conserven el equipo que eligieron con `/pvpteam` al entrar al evento global, incluso si eso hace que los equipos queden disparejos.

> **Nota:** Modos soportados: `FFA` (todos contra todos), `2vs2`, `3vs3vs4`, `4vs4vs5vs2`, `1vs1vs2vs7vs10`, etc (versus de hasta 5 equipos diferentes sin limite de miembros). Recompensas soportadas: Ej. `50s` (50 plata), `1g` (1 oro), `1p` (1 platino), `none`(ninguna).


## 🔧 Configuración (`PvPterraConfig.json`)

El archivo JSON permite un control granular de la experiencia global y de cada región individual. Se puede modificar en vivo y usar el comando `/reload` de TShock para aplicar los cambios sin reiniciar el servidor.

```json
{
  "PvPdefaultLang": "es",          // Idioma por defecto del plugin (es, en, pt).
  "DefaultReward": "none",         // Recompensa o costo base por baja (ej: 50s, 1g).
  "AllowBackOnDeath": false,       // Permitir que el jugador muerto vuelva a la batalla mientras su equipo siga vivo.
  "DefaultPvPBuffs": [],           // IDs de buffs que se aplicarán a todos los jugadores por defecto.
  "CleanBuffsOnEnter": true,       // Quitar todos los buffs activos al entrar a la zona PvP.
  "CombatTagDurationSeconds": 15,  // Tiempo para considerar que un jugador sigue "en batalla" y penalizar la huida.
  "PvPHealthActive": false,        // Activar el límite de vida máxima dentro de las arenas.
  "PvPHealth": 400,                // Cantidad de vida máxima (se redondea a múltiplos de 20).
  "PvPManaActive": false,          // Activar el límite de maná máximo dentro de las arenas.
  "PvPMana": 200,                  // Cantidad de maná máximo (se redondea a múltiplos de 20).
  "PvPinventoryActive": false,     // Definir si se usará un inventario especial (Loadout) al entrar.
  "PvPinventoryItems": [],         // Items del inventario PvP (Formato "ID:Stack" o "ID").
  "PvPItemsRestore": "none",       // Tiempo para reabastecer items automáticamente (ej: "10s", "1m").
  "PvPinventoryPotions": [],       // Pociones específicas para el inventario PvP.
  "PvPotionsRestore": "none",      // Tiempo para reabastecer pociones automáticamente (ej: "30s").
  "AllowPickUp": false,            // Permitir recoger objetos del suelo (Anti-contrabando si es false).
  "AllowDrop": false,              // Permitir tirar objetos al suelo dentro de la arena.


  "RegionConfigs": {               // Configuraciones específicas por región.
    "test": {                      // Nombre de la Región.
      "Mode": "2vs2",              // Modo de juego de esta región específica.
      "PvPreward": "20s",          // Recompensa o costo de entrada para esta arena.
      "AllowBackOnDeath": true,    // Opción de reingreso tras morir específica para esta zona.
      "PvPbuffs": [],              // Buffs exclusivos que solo se dan en esta región.
      "PvPHealthActive": false,
      "PvPHealth": 400,
      "PvPManaActive": false,
      "PvPMana": 200,
      "PvPinventoryActive": false,
      "PvPinventoryItems": [],
      "PvPItemsRestore": "none",
      "PvPinventoryPotions": [],
      "PvPotionsRestore": "none",
      "AllowPickUp": false,
      "AllowDrop": false
    }
  }
}
```

---
# ENGLISH

> **NOTE: SSC (Server Side Characters) MUST BE ENABLED FOR THE PLUGIN TO WORK CORRECTLY**

PvPterraUtils is an advanced PvP management, arena, and strict inventory control system for Terraria servers (TShock). Designed to create fair and balanced competitive experiences.

## ✨ Key Features

* **⚔️ Dynamic Arenas (FFA & Teams):** Support for "Free For All" modes and team battles (Red, Blue, Green, Yellow, Pink).
* **🛡️ PvP Region Management:** Allows you to designate regions with different PvP settings and characteristics, all configurable via the `.json` file.
* **💰 Integrated Economy:** Charges entry fees for entering competitive regions and grants configurable rewards per kill.
* **🔥 Killstreaks:** Global chat announcements for notable streaks (Triple, Penta, Unstoppable, Godlike).
* **👀 Smart Spectator Mode:** Spectate while waiting for a battle to end or by choice using the `/pvpteam spectator` command.
* **🔄 Tactical Re-entry (AllowBackOnDeath):** Configurable option to allow dead players to respawn and return directly to an ongoing team battle.
* **🌍 Multi-language Support (i18n):** Native and automatic translation for all messages and logs in **Spanish (es)**, **English (en)**, and **Portuguese (pt)**.
* **📦 Custom Inventories:** Define exact amounts of ammo or consumables for the arena. When a player enters, their inventory is automatically swapped to your pre-configured setup.
* **🏆 Ranking:** Global and arena ranking system based on deaths.

## 📜 Player Commands

| Command | Parameters | Permission | Description |
| :--- | :--- | :--- | :--- |
| `/pvp info` | None | `pvpterra.user` | Displays info about the current zone (mode, reward, remaining time). |
| `/pvpranking` | `[region]` | `pvpterra.ranking` | Displays the Top 10 players (Global or by specific region). |
| `/pvpteam` | `<color/none/spectator>`| `pvpterra.team` | Sets team preference (red, blue, pink, etc.) before entering an arena. |

## 🛠️ Admin Commands

*All admin commands require the following permission:* `pvpterra.admin`

| Command | Parameters | Description |
| :--- | :--- | :--- |
| `/pvpactive` | `[t/f] [mode] [reward] [--forceteam / --ignoreteam]` | Enables or disables forced Global PvP across the map. |
| `/pvpregion assign` | `[name] [mode] [reward]` | Converts an existing TShock region into a PvP Arena. |
| `/pvpregion define` | `[name] [mode] [reward]` | Creates a new TShock region and defines it as a PvP Arena. |
| `/pvpregion redefine`| `[name] [mode] [reward]` | Updates the rules (mode/reward) for an existing arena. |
| `/pvpregion remove` | `[name]` | Removes PvP properties from a region (the physical region remains). |
| `/pvpregion delete` | `[name]` | Completely destroys the region from the server and the plugin. |
| `/pvpregion set` | `[1/2]` | Selection tool to create regions with `/pvpregion define`. |

> **Global PvP Flags (/pvpactive):**
> * `--forceteam`: Ignores player preferences and forces an even split across teams required by the mode.
> * `--ignoreteam`: Allows players to keep the team chosen via `/pvpteam` during the global event.

> **Note:** Supported modes include `FFA`, `2vs2`, `3vs3vs4`, etc. (battles with up to 5 teams). Supported rewards: e.g., `50s` (50 silver), `1g` (1 gold), `1p` (1 platinum), or `none`.

## 🔧 Configuration (PvPterraConfig.json)

The JSON file allows granular control over the global experience and individual regions. Use `/reload` to apply changes without restarting the server.

```json
{
  "PvPdefaultLang": "es",          // Default plugin language (es, en, pt).
  "DefaultReward": "none",         // Base reward or entry cost per kill (e.g., 50s, 1g).
  "AllowBackOnDeath": false,       // Allow dead players to return to battle while teammates are alive.
  "DefaultPvPBuffs": [],           // Buff IDs applied to all players by default.
  "CleanBuffsOnEnter": true,       // Remove all active buffs when entering a PvP zone.
  "CombatTagDurationSeconds": 15,  // Duration to consider a player "in combat" for flee penalties.
  "PvPHealthActive": false,        // Enable maximum health limits within arenas.
  "PvPHealth": 400,                // Max health amount (rounded to multiples of 20).
  "PvPManaActive": false,          // Enable maximum mana limits within arenas.
  "PvPMana": 200,                  // Max mana amount (rounded to multiples of 20).
  "PvPinventoryActive": false,     // Define if a special Loadout will be used upon entry.
  "PvPinventoryItems": [],         // PvP inventory items (Format: "ID:Stack" or "ID").
  "PvPItemsRestore": "none",       // Auto-restock time for items (e.g., "10s", "1m").
  "PvPinventoryPotions": [],       // Specific potions for the PvP inventory.
  "PvPotionsRestore": "none",      // Auto-restock time for potions (e.g., "30s").
  "AllowPickUp": false,            // Allow picking up items from the ground (Anti-contraband if false).
  "AllowDrop": false,              // Allow dropping items on the ground within the arena.

  "RegionConfigs": {               // Region-specific configurations.
    "test": {                      // Region Name.
      "Mode": "2vs2",              // Game mode for this specific region.
      "PvPreward": "20s",          // Reward or entry cost for this arena.
      "AllowBackOnDeath": true,    // Region-specific re-entry option after death.
      "PvPbuffs": [],              // Exclusive buffs granted only in this region.
      "PvPHealthActive": false,
      "PvPHealth": 400,
      "PvPManaActive": false,
      "PvPMana": 200,
      "PvPinventoryActive": false,
      "PvPinventoryItems": [],
      "PvPItemsRestore": "none",
      "PvPinventoryPotions": [],
      "PvPotionsRestore": "none",
      "AllowPickUp": false,
      "AllowDrop": false
    }
  }
}
