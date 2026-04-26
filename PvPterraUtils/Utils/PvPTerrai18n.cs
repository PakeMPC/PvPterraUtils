using System;
using System.Collections.Generic;
using PvPterraUtils.Data;
using TShockAPI;

namespace PvPterraUtils.utils
{
    public class PvPTerrai18n
    {
        // Default Language
        public static string CurrentLang = "en";

        private static Dictionary<string, Dictionary<string, string>> _texts = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            // ==========================================
            // ESPAÑOL (ES)
            // ==========================================
            { "es", new Dictionary<string, string>
                {
                    { "PvP_EnterZone", "[c/FF0000:Entraste en la zona PvP: {0}. El modo es {1}.]" },
                    { "Cmd_NotInPvP", "Actualmente no te encuentras en ninguna zona PvP." },
                    { "Cmd_TeamSet", "Preferencia de equipo establecida a: {0}." },
                    { "Cmd_TeamCleared", "Preferencia de equipo eliminada. Jugarás en modo FFA." },
                    { "Cmd_InvalidTeam", "Equipo no válido. Usa: red, blue, green, yellow, pink, spectator o none." },

                    // --- Mensajes del Núcleo (Core) ---
                    { "Log_LeaveCombat", "[PvPterra] {0} se desconectó en combate. Kill otorgada a {1}." },
                    { "Core_FleeReward", "[PvPterra] {0} huyó del combate. ¡Has recibido la kill!" },
                    { "Core_ReloadSuccess", "[PvPterra] Configuración JSON recargada correctamente desde el disco." },
                    { "Core_TeamReassigned", "[PvPterra] Espacios insuficientes en el equipo {0}. Se te reasignó a {1}." },

                    //Racha de kills
                    { "Streak_3", "¡{0} hizo una triple kill!" },
                    { "Streak_5", "¡{0} ha hecho una penta kill!" },
                    { "Streak_10", "¡{0} es IMPARABLE! (10 muertes)" },
                    { "Streak_15", "¡{0} es DIVINO! (15 muertes)" },
                    { "Streak_20", "¡{0} es LEGENDARIO! (20 muertes)" },
                    { "Streak_End", "¡{0} ha terminado con la racha de {1}!" },

                    // --- Registros de Sistema (Consola) ---
                    { "Log_ConfigNew", "[PvPterra] Generando nuevo config.json con todas las opciones..." },
                    { "Log_ConfigLoadError", "[PvPterra] Error crítico al cargar JSON: {0}" },
                    { "Log_ConfigSaveError", "[PvPterra] Error al guardar JSON: {0}" },

                    // --- Mensajes de Combate y Bloqueos ---
                    { "Combat_TeamLocked", "[PvPterra] Tu equipo está bloqueado por las reglas de la arena. (Asignado: {0})" },
                    { "Combat_LoadoutLocked", "[PvPterra] No puedes cambiar de carga (Loadout) en la arena." },
                    { "Combat_SlotLocked", "[PvPterra] Este slot está bloqueado en la arena." },
                    { "Combat_InventoryLocked", "[PvPterra] No puedes equipar objetos de tu inventario personal." },
                    { "Combat_PvPLocked", "[PvPterra] No puedes desactivar el PvP mientras estés en la arena." },
                    { "Combat_KillReward", "[PvPterra] ¡Has recibido {0} por derrotar a {1}!" },
                    { "Combat_DropLocked", "[PvPterra] Reglas de arena: No puedes arrojar objetos al suelo." },
                    { "Combat_PickupLocked", "[PvPterra] Reglas de arena: El contrabando está prohibido." },

                    // --- Comandos de Usuario ---
                    { "Cmd_Info_Error", "Error al leer la información de la región actual." },
                    { "Cmd_Info_Title", "--- Información PvP: {0} ---" },
                    { "Cmd_Info_Mode", "Modo de Juego: {0}" },
                    { "Cmd_Info_Reward", "Recompensa por Kill / Costo de Entrada: {0}" },
                    { "Cmd_Info_Team", "Equipo Actual: {0}" },
                    { "Cmd_Info_TeamFFA", "Sin Equipo (FFA)" },
                    { "Cmd_Info_InCombat", "¡ESTÁS EN COMBATE! ({0}s restantes)" },
                    { "Cmd_Info_Attacker", "Último agresor: {0}" },
                    { "Cmd_Info_Safe", "Estado: Seguro (Puedes salir de la región sin penalización)." },
                    { "Cmd_Rank_Empty", "El ranking está vacío. ¡Ve a derramar algo de sangre!" },
                    { "Cmd_Rank_Global", "-- TOP 10 PvP GLOBAL --" },
                    { "Cmd_Rank_Region", "-- TOP 10 PvP ({0}) --" },
                    { "Cmd_Team_Syntax", "Sintaxis incorrecta. Uso: /pvpteam [color/none/spectator]" },
                    { "Cmd_Team_Options", "Opciones: red, blue, green, yellow, pink, none, spectator" },

                    // --- Comandos de Admin ---
                    { "Cmd_Active_Syntax", "Uso: /pvpactive [t/f] [modo] [recompensa] [--ignoreteam / --forceteam]" },
                    { "Cmd_Active_Off", "[PvPterra] El PvP Global ha sido desactivado." },
                    { "Cmd_Active_On", "[PvPterra] ¡PVP GLOBAL ACTIVADO!" },
                    { "Cmd_Active_Status", "Modo: {0} | Recompensa: {1}" },
                    { "Cmd_Active_Force", "[PvPterra] ¡Has sido forzado al combate global! Tu equipo es {0}." },
                    { "Cmd_Reg_NotExists", "La región '{0}' no existe. Créala primero o usa /pvpregion define." },
                    { "Cmd_Reg_Assign_Success", "[PvPterra] '{0}' asignada. Modo: {1} | Recompensa: {2}." },
                    { "Cmd_Reg_Define_Success", "[PvPterra] Creada y definida. Modo: {0} | Recompensa: {1}." },
                    { "Cmd_Reg_Remove_Success", "[PvPterra] Zona PvP removida de '{0}'. (La región física sigue intacta)." },
                    { "Cmd_Reg_Remove_Error", "La región '{0}' no estaba asignada como zona PvP." },
                    { "Cmd_Reg_Redefine_Success", "[PvPterra] Región '{0}' actualizada -> Modo: {1} | Recompensa: {2}." },
                    { "Cmd_Reg_Redefine_Error", "La región '{0}' no es una zona PvP. Usa /pvpregion assign primero." },
                    { "Cmd_Reg_Del_Success", "[PvPterra] Región física '{0}' DESTRUIDA del mapa y eliminada del PvP." },
                    { "Cmd_Reg_Invalid", "Subcomando no reconocido. Escribe /pvpregion para ver la ayuda." },

                    // --- Gestión de Inventario y Economía ---
                    { "Inv_StoneEquipped", "[PvPterra] Se te ha equipado una Piedra de Carga para bloquear el contrabando." },
                    { "Inv_RecoverSuccess", "[PvPterra] ¡Tu inventario original ha sido recuperado de un respaldo tras el cierre del servidor!" },
                    { "Inv_RestoreSuccess", "[PvPterra] Tu equipo y accesorios originales han sido restaurados." },
                    { "Inv_RewardPending", "[c/FFD700:[PvPterra] ¡Tus ganancias ({0}) han sido añadidas a tu inventario!]" },
                    { "Inv_RestockSuccess", "[c/00FF00:[PvPterra] ¡Tus suministros han sido reabastecidos!]" },

                    // --- Mensajes de Región y Estado de Partida ---
                    { "Reg_RespawnRestore", "[PvPterra] Has reaparecido. Tu inventario original ha sido restaurado." },
                    { "Reg_SpectatorEliminated", "[PvPterra] Has sido eliminado. Modo espectador activado (Usa la Scrying Orb)." },
                    { "Reg_FleeKill", "[PvPterra] ¡Fuiste asesinado por intentar huir del combate!" },
                    { "Reg_LeaveRestore", "[PvPterra] Saliste de la zona PvP ({0}). Tu inventario ha sido restaurado." },
                    { "Reg_LeaveNoRestore", "[PvPterra] Saliste de la zona PvP ({0})." },
                    { "Reg_FFANeedFunds", "[PvPterra] Necesitas {0} para entrar al FFA." },
                    { "Reg_FFAEnter", "[PvPterra] ¡Has entrado al combate FFA!" },
                    { "Reg_WaitingPlayers", "[PvPterra] Esperando jugadores para {0} ({1}/{2})" },
                    { "Reg_KickNoFunds", "[PvPterra] Fuiste expulsado de la sala por no tener los {0} requeridos." },
                    { "Reg_MatchAbortedFunds", "[PvPterra] Un jugador no tenía fondos suficientes. Volviendo a la sala de espera..." },
                    { "Reg_MatchStarted", "[PvPterra] ¡Cupos llenos ({0}/{0})! ¡La partida {1} ha comenzado!" },
                    { "Reg_MatchEndWinner", "[c/FFD700:[PvPterra] ¡Batalla terminada! Ganadores: Equipo {0} ({1})]" },
                    { "Reg_MatchEndTie", "[c/FFD700:[PvPterra] ¡La batalla ha terminado en Empate!]" },
                    { "Reg_ResetTimer", "[PvPterra] Activando sala de espera en 5 segundos..." },
                    { "Reg_TeamRestored", "[PvPterra] ¡Tu equipo ha sido restaurado a la fuerza por la arena!" },
                    { "Reg_SpectatorInCourse", "[PvPterra] Hay una batalla en curso, entraste como espectador." },
                    { "Reg_SpectatorPref", "[PvPterra] Entraste en modo espectador (Preferencia guardada)." },
                    { "Reg_TeamFull", "[PvPterra] No puedes entrar. El equipo {0} ya está lleno o no participa." },
                    { "Reg_ArenaFull", "[PvPterra] No puedes entrar. Todas las vacantes de combate para {0} están llenas." },
                    { "Reg_InternalError", "[PvPterra] Error interno al activar tu PvP. Revisa la consola." },
                    { "Reg_StrictBuffs", "Los buffs de esta región son estrictos." },
                    { "Log_BuffError", "[PvPterra] Error en EnforcePvPBuffs: {0}" },
                    { "Log_EnterRegionError", "[PvPterra] ERROR CRÍTICO en EnterPvPRegion: {0}\n{1}" },

                    // --- Ayuda de Comandos de Región ---
                    { "Cmd_Reg_Help_Exist", "Opciones para regiones existentes:" },
                    { "Cmd_Reg_Help_Assign", "/pvpregion assign [nombre] [modo] [recompensa?]" },
                    { "Cmd_Reg_Help_Remove", "/pvpregion remove [nombre]" },
                    { "Cmd_Reg_Help_Redefine", "/pvpregion redefine [nombre] [modo] [recompensa]" },
                    { "Cmd_Reg_Help_New", "Opciones para crear regiones desde cero:" },
                    { "Cmd_Reg_Help_Set", "/pvpregion set [1/2]" },
                    { "Cmd_Reg_Help_Define", "/pvpregion define [nombre] [modo] [recompensa?]" },
                    { "Cmd_Reg_Help_Delete", "/pvpregion delete [nombre]" },

                    // --- Respuestas de Comandos de Región ---
                    { "Cmd_Reg_Assign_Syntax", "Uso: /pvpregion assign [nombre] [modo] [recompensa?]" },
                    { "Cmd_Reg_NotExist", "La región '{0}' no existe. Créala primero o usa /pvpregion define." },
                    { "Cmd_Reg_Assigned", "[PvPterra] '{0}' asignada. Modo: {1} | Recompensa: {2}." },

                    { "Cmd_Reg_Define_Syntax", "Uso: /pvpregion define [nombre] [modo] [recompensa]" },
                    { "Cmd_Reg_Defined", "[PvPterra] Creada y definida. Modo: {0} | Recompensa: {1}." },

                    { "Cmd_Reg_Remove_Syntax", "Uso: /pvpregion remove [nombre]" },
                    { "Cmd_Reg_Removed", "[PvPterra] Zona PvP removida de '{0}'. (La región física sigue intacta)." },
                    { "Cmd_Reg_NotAssigned", "La región '{0}' no estaba asignada como zona PvP." },

                    { "Cmd_Reg_Redefine_Syntax", "Uso: /pvpregion redefine [nombre] [XvsX/FFA] [Recompensa]" },
                    { "Cmd_Reg_Redefine_Example", "Ejemplo: /pvpregion redefine Arena 2vs2 5g" },
                    { "Cmd_Reg_Redefined", "[PvPterra] Región '{0}' actualizada -> Modo: {1} | Recompensa: {2}." },

                    { "Cmd_Reg_Set_Syntax", "Uso: /pvpregion set [1/2]" },

                    { "Cmd_Reg_Delete_Syntax", "Uso: /pvpregion delete [nombre]" },
                    { "Cmd_Reg_Deleted", "[PvPterra] Región física '{0}' DESTRUIDA del mapa y eliminada del PvP." },

                    { "Cmd_Reg_Unknown", "Subcomando no reconocido. Escribe /pvpregion para ver la ayuda." },
                }
            },

            // ==========================================
            // ENGLISH (EN)
            // ==========================================
            { "en", new Dictionary<string, string>
                {
                    { "PvP_EnterZone", "[c/FF0000:You entered the PvP zone: {0}. Mode is {1}.]" },
                    { "Cmd_NotInPvP", "You are not currently in any PvP zone." },
                    { "Cmd_TeamSet", "Team preference set to: {0}." },
                    { "Cmd_TeamCleared", "Team preference cleared. You will play in FFA mode." },
                    { "Cmd_InvalidTeam", "Invalid team. Use: red, blue, green, yellow, pink, spectator or none." },

                    // --- Core Messages ---
                    { "Log_LeaveCombat", "[PvPterra] {0} disconnected during combat. Kill awarded to {1}." },
                    { "Core_FleeReward", "[PvPterra] {0} fled from combat. You received the kill!" },
                    { "Core_ReloadSuccess", "[PvPterra] JSON configuration reloaded successfully from disk." },
                    { "Core_TeamReassigned", "[PvPterra] Not enough slots in team {0}. You were reassigned to {1}." },

                    // Kill Streaks
                    { "Streak_3", "{0} got a triple kill!" },
                    { "Streak_5", "{0} got a penta kill!" },
                    { "Streak_10", "{0} is UNSTOPPABLE! (10 kills)" },
                    { "Streak_15", "{0} is DIVINE! (15 kills)" },
                    { "Streak_20", "{0} is LEGENDARY! (20 kills)" },
                    { "Streak_End", "{0} has ended {1}'s kill streak!" },

                    // --- System Logs (Console) ---
                    { "Log_ConfigNew", "[PvPterra] Generating new config.json with all options..." },
                    { "Log_ConfigLoadError", "[PvPterra] Critical error loading JSON: {0}" },
                    { "Log_ConfigSaveError", "[PvPterra] Error saving JSON: {0}" },

                    // --- Combat and Block Messages ---
                    { "Combat_TeamLocked", "[PvPterra] Your team is locked by arena rules. (Assigned: {0})" },
                    { "Combat_LoadoutLocked", "[PvPterra] You cannot change loadouts in the arena." },
                    { "Combat_SlotLocked", "[PvPterra] This slot is locked in the arena." },
                    { "Combat_InventoryLocked", "[PvPterra] You cannot equip items from your personal inventory." },
                    { "Combat_PvPLocked", "[PvPterra] You cannot disable PvP while in the arena." },
                    { "Combat_KillReward", "[PvPterra] You received {0} for defeating {1}!" },
                    { "Combat_DropLocked", "[PvPterra] Arena rules: You cannot drop items on the ground." },
                    { "Combat_PickupLocked", "[PvPterra] Arena rules: Contraband is prohibited." },

                    // --- User Commands ---
                    { "Cmd_Info_Error", "Error reading current region information." },
                    { "Cmd_Info_Title", "--- PvP Information: {0} ---" },
                    { "Cmd_Info_Mode", "Game Mode: {0}" },
                    { "Cmd_Info_Reward", "Kill Reward / Entry Cost: {0}" },
                    { "Cmd_Info_Team", "Current Team: {0}" },
                    { "Cmd_Info_TeamFFA", "No Team (FFA)" },
                    { "Cmd_Info_InCombat", "YOU ARE IN COMBAT! ({0}s remaining)" },
                    { "Cmd_Info_Attacker", "Last attacker: {0}" },
                    { "Cmd_Info_Safe", "Status: Safe (You can leave the region without penalty)." },
                    { "Cmd_Rank_Empty", "The ranking is empty. Go spill some blood!" },
                    { "Cmd_Rank_Global", "-- TOP 10 GLOBAL PvP --" },
                    { "Cmd_Rank_Region", "-- TOP 10 PvP ({0}) --" },
                    { "Cmd_Team_Syntax", "Incorrect syntax. Usage: /pvpteam [color/none/spectator]" },
                    { "Cmd_Team_Options", "Options: red, blue, green, yellow, pink, none, spectator" },

                    // --- Admin Commands ---
                    { "Cmd_Active_Syntax", "Usage: /pvpactive [t/f] [mode] [reward] [--ignoreteam / --forceteam]" },
                    { "Cmd_Active_Off", "[PvPterra] Global PvP has been disabled." },
                    { "Cmd_Active_On", "[PvPterra] GLOBAL PVP ACTIVATED!" },
                    { "Cmd_Active_Status", "Mode: {0} | Reward: {1}" },
                    { "Cmd_Active_Force", "[PvPterra] You have been forced into global combat! Your team is {0}." },
                    { "Cmd_Reg_NotExists", "Region '{0}' does not exist. Create it first or use /pvpregion define." },
                    { "Cmd_Reg_Assign_Success", "[PvPterra] '{0}' assigned. Mode: {1} | Reward: {2}." },
                    { "Cmd_Reg_Define_Success", "[PvPterra] Created and defined. Mode: {0} | Reward: {1}." },
                    { "Cmd_Reg_Remove_Success", "[PvPterra] PvP zone removed from '{0}'. (Physical region remains intact)." },
                    { "Cmd_Reg_Remove_Error", "Region '{0}' was not assigned as a PvP zone." },
                    { "Cmd_Reg_Redefine_Success", "[PvPterra] Region '{0}' updated -> Mode: {1} | Reward: {2}." },
                    { "Cmd_Reg_Redefine_Error", "Region '{0}' is not a PvP zone. Use /pvpregion assign first." },
                    { "Cmd_Reg_Del_Success", "[PvPterra] Physical region '{0}' DESTROYED from the map and removed from PvP." },
                    { "Cmd_Reg_Invalid", "Unrecognized subcommand. Type /pvpregion for help." },

                    // --- Inventory and Economy Management ---
                    { "Inv_StoneEquipped", "[PvPterra] An Encumbering Stone has been equipped to block contraband." },
                    { "Inv_RecoverSuccess", "[PvPterra] Your original inventory has been recovered from a backup after server shutdown!" },
                    { "Inv_RestoreSuccess", "[PvPterra] Your original equipment and accessories have been restored." },
                    { "Inv_RewardPending", "[c/FFD700:[PvPterra] Your earnings ({0}) have been added to your inventory!]" },
                    { "Inv_RestockSuccess", "[c/00FF00:[PvPterra] Your supplies have been restocked!]" },

                    // --- Region and Match Status Messages ---
                    { "Reg_RespawnRestore", "[PvPterra] You have respawned. Your original inventory has been restored." },
                    { "Reg_SpectatorEliminated", "[PvPterra] You have been eliminated. Spectator mode activated (Use the Scrying Orb)." },
                    { "Reg_FleeKill", "[PvPterra] You were killed for trying to flee from combat!" },
                    { "Reg_LeaveRestore", "[PvPterra] You left the PvP zone ({0}). Your inventory has been restored." },
                    { "Reg_LeaveNoRestore", "[PvPterra] You left the PvP zone ({0})." },
                    { "Reg_FFANeedFunds", "[PvPterra] You need {0} to enter FFA." },
                    { "Reg_FFAEnter", "[PvPterra] You have entered FFA combat!" },
                    { "Reg_WaitingPlayers", "[PvPterra] Waiting for players for {0} ({1}/{2})" },
                    { "Reg_KickNoFunds", "[PvPterra] You were kicked from the room for not having the required {0}." },
                    { "Reg_MatchAbortedFunds", "[PvPterra] A player did not have enough funds. Returning to the waiting room..." },
                    { "Reg_MatchStarted", "[PvPterra] Slots filled ({0}/{0})! The {1} match has started!" },
                    { "Reg_MatchEndWinner", "[c/FFD700:[PvPterra] Battle finished! Winners: Team {0} ({1})]" },
                    { "Reg_MatchEndTie", "[c/FFD700:[PvPterra] The battle ended in a Tie!]" },
                    { "Reg_ResetTimer", "[PvPterra] Activating waiting room in 5 seconds..." },
                    { "Reg_TeamRestored", "[PvPterra] Your team has been forcibly restored by the arena!" },
                    { "Reg_SpectatorInCourse", "[PvPterra] There is a battle in progress, you entered as a spectator." },
                    { "Reg_SpectatorPref", "[PvPterra] Entered spectator mode (Preference saved)." },
                    { "Reg_TeamFull", "[PvPterra] Cannot enter. Team {0} is full or not participating." },
                    { "Reg_ArenaFull", "[PvPterra] Cannot enter. All combat slots for {0} are full." },
                    { "Reg_InternalError", "[PvPterra] Internal error activating your PvP. Check the console." },
                    { "Reg_StrictBuffs", "Buffs in this region are strict." },
                    { "Log_BuffError", "[PvPterra] Error in EnforcePvPBuffs: {0}" },
                    { "Log_EnterRegionError", "[PvPterra] CRITICAL ERROR in EnterPvPRegion: {0}\n{1}" },

                    // --- Region Command Help ---
                    { "Cmd_Reg_Help_Exist", "Options for existing regions:" },
                    { "Cmd_Reg_Help_Assign", "/pvpregion assign [name] [mode] [reward?]" },
                    { "Cmd_Reg_Help_Remove", "/pvpregion remove [name]" },
                    { "Cmd_Reg_Help_Redefine", "/pvpregion redefine [name] [mode] [reward]" },
                    { "Cmd_Reg_Help_New", "Options for creating regions from scratch:" },
                    { "Cmd_Reg_Help_Set", "/pvpregion set [1/2]" },
                    { "Cmd_Reg_Help_Define", "/pvpregion define [name] [mode] [reward?]" },
                    { "Cmd_Reg_Help_Delete", "/pvpregion delete [name]" },

                    // --- Region Command Responses ---
                    { "Cmd_Reg_Assign_Syntax", "Usage: /pvpregion assign [name] [mode] [reward?]" },
                    { "Cmd_Reg_NotExist", "The region '{0}' does not exist. Create it first or use /pvpregion define." },
                    { "Cmd_Reg_Assigned", "[PvPterra] '{0}' assigned. Mode: {1} | Reward: {2}." },

                    { "Cmd_Reg_Define_Syntax", "Usage: /pvpregion define [name] [mode] [reward]" },
                    { "Cmd_Reg_Defined", "[PvPterra] Created and defined. Mode: {0} | Reward: {1}." },

                    { "Cmd_Reg_Remove_Syntax", "Usage: /pvpregion remove [name]" },
                    { "Cmd_Reg_Removed", "[PvPterra] PvP zone removed from '{0}'. (The physical region remains intact)." },
                    { "Cmd_Reg_NotAssigned", "The region '{0}' was not assigned as a PvP zone." },

                    { "Cmd_Reg_Redefine_Syntax", "Usage: /pvpregion redefine [name] [XvsX/FFA] [Reward]" },
                    { "Cmd_Reg_Redefine_Example", "Example: /pvpregion redefine Arena 2vs2 5g" },
                    { "Cmd_Reg_Redefined", "[PvPterra] Region '{0}' updated -> Mode: {1} | Reward: {2}." },

                    { "Cmd_Reg_Set_Syntax", "Usage: /pvpregion set [1/2]" },

                    { "Cmd_Reg_Delete_Syntax", "Usage: /pvpregion delete [name]" },
                    { "Cmd_Reg_Deleted", "[PvPterra] Physical region '{0}' DESTROYED from the map and removed from PvP." },

                    { "Cmd_Reg_Unknown", "Unrecognized subcommand. Type /pvpregion to see help." },
                }
            },

            // ==========================================
            // PORTUGUÊS (PT)
            // ==========================================
            { "pt", new Dictionary<string, string>
                {
                    { "PvP_EnterZone", "[c/FF0000:Entraste na zona PvP: {0}. O modo é {1}.]" },
                    { "Cmd_NotInPvP", "Atualmente não te encontras em nenhuma zona PvP." },
                    { "Cmd_TeamSet", "Preferência de equipa definida para: {0}." },
                    { "Cmd_TeamCleared", "Preferência de equipa removida. Vais jogar no modo FFA." },
                    { "Cmd_InvalidTeam", "Equipa inválida. Usa: red, blue, green, yellow, pink, spectator ou none." },

                    // --- Mensagens do Núcleo (Core) ---
                    { "Log_LeaveCombat", "[PvPterra] {0} desconectou-se em combate. Kill concedida a {1}." },
                    { "Core_FleeReward", "[PvPterra] {0} fugiu do combate. Recebeste a kill!" },
                    { "Core_ReloadSuccess", "[PvPterra] Configuração JSON recarregada corretamente do disco." },
                    { "Core_TeamReassigned", "[PvPterra] Espaços insuficientes na equipa {0}. Foste reatribuído a {1}." },

                    // Racha de kills
                    { "Streak_3", "¡{0} fez uma triple kill!" },
                    { "Streak_5", "¡{0} fez uma penta kill!" },
                    { "Streak_10", "¡{0} é IMPARÁVEL! (10 mortes)" },
                    { "Streak_15", "¡{0} é DIVINO! (15 mortes)" },
                    { "Streak_20", "¡{0} é LENDÁRIO! (20 mortes)" },
                    { "Streak_End", "¡{0} terminou com a racha de {1}!" },

                    // --- Registos de Sistema (Consola) ---
                    { "Log_ConfigNew", "[PvPterra] Gerando novo config.json com todas as opções..." },
                    { "Log_ConfigLoadError", "[PvPterra] Erro crítico ao carregar JSON: {0}" },
                    { "Log_ConfigSaveError", "[PvPterra] Erro ao guardar JSON: {0}" },

                    // --- Mensagens de Combate e Bloqueios ---
                    { "Combat_TeamLocked", "[PvPterra] A tua equipa está bloqueada pelas regras da arena. (Atribuída: {0})" },
                    { "Combat_LoadoutLocked", "[PvPterra] Não podes mudar de carga (Loadout) na arena." },
                    { "Combat_SlotLocked", "[PvPterra] Este slot está bloqueado na arena." },
                    { "Combat_InventoryLocked", "[PvPterra] Não podes equipar itens do teu inventário pessoal." },
                    { "Combat_PvPLocked", "[PvPterra] Não podes desativar o PvP enquanto estiveres na arena." },
                    { "Combat_KillReward", "[PvPterra] Recebeste {0} por derrotar {1}!" },
                    { "Combat_DropLocked", "[PvPterra] Regras da arena: Não podes deitar itens ao chão." },
                    { "Combat_PickupLocked", "[PvPterra] Regras da arena: O contrabando é proibido." },

                    // --- Comandos de Utilizador ---
                    { "Cmd_Info_Error", "Erro ao ler a informação da região atual." },
                    { "Cmd_Info_Title", "--- Informação PvP: {0} ---" },
                    { "Cmd_Info_Mode", "Modo de Jogo: {0}" },
                    { "Cmd_Info_Reward", "Recompensa por Kill / Custo de Entrada: {0}" },
                    { "Cmd_Info_Team", "Equipa Atual: {0}" },
                    { "Cmd_Info_TeamFFA", "Sem Equipa (FFA)" },
                    { "Cmd_Info_InCombat", "¡ESTÁS EM COMBATE! ({0}s restantes)" },
                    { "Cmd_Info_Attacker", "Último agressor: {0}" },
                    { "Cmd_Info_Safe", "Estado: Seguro (Podes sair da região sem penalização)." },
                    { "Cmd_Rank_Empty", "O ranking está vazio. Vai derramar algum sangue!" },
                    { "Cmd_Rank_Global", "-- TOP 10 PvP GLOBAL --" },
                    { "Cmd_Rank_Region", "-- TOP 10 PvP ({0}) --" },
                    { "Cmd_Team_Syntax", "Sintaxe incorreta. Uso: /pvpteam [color/none/spectator]" },
                    { "Cmd_Team_Options", "Opções: red, blue, green, yellow, pink, none, spectator" },

                    // --- Comandos de Admin ---
                    { "Cmd_Active_Syntax", "Uso: /pvpactive [t/f] [modo] [recompensa] [--ignoreteam / --forceteam]" },
                    { "Cmd_Active_Off", "[PvPterra] O PvP Global foi desativado." },
                    { "Cmd_Active_On", "[PvPterra] ¡PVP GLOBAL ATIVADO!" },
                    { "Cmd_Active_Status", "Modo: {0} | Recompensa: {1}" },
                    { "Cmd_Active_Force", "[PvPterra] Foste forçado ao combate global! A tua equipa é {0}." },
                    { "Cmd_Reg_NotExists", "A região '{0}' não existe. Cria-a primeiro ou usa /pvpregion define." },
                    { "Cmd_Reg_Assign_Success", "[PvPterra] '{0}' atribuída. Modo: {1} | Recompensa: {2}." },
                    { "Cmd_Reg_Define_Success", "[PvPterra] Criada e definida. Modo: {0} | Recompensa: {1}." },
                    { "Cmd_Reg_Remove_Success", "[PvPterra] Zona PvP removida de '{0}'. (A região física permanece intacta)." },
                    { "Cmd_Reg_Remove_Error", "A região '{0}' não estava atribuída como zona PvP." },
                    { "Cmd_Reg_Redefine_Success", "[PvPterra] Região '{0}' atualizada -> Modo: {1} | Recompensa: {2}." },
                    { "Cmd_Reg_Redefine_Error", "A região '{0}' não é uma zona PvP. Usa /pvpregion assign primeiro." },
                    { "Cmd_Reg_Del_Success", "[PvPterra] Região física '{0}' DESTRUÍDA do mapa e eliminada do PvP." },
                    { "Cmd_Reg_Invalid", "Subcomando não reconhecido. Escreve /pvpregion para ajuda." },

                    // --- Gestão de Inventário e Economia ---
                    { "Inv_StoneEquipped", "[PvPterra] Foi-te equipada uma Pedra de Carga para bloquear o contrabando." },
                    { "Inv_RecoverSuccess", "[PvPterra] O teu inventário original foi recuperado de um backup após o fecho do servidor!" },
                    { "Inv_RestoreSuccess", "[PvPterra] O teu equipamento e acessórios originais foram restaurados." },
                    { "Inv_RewardPending", "[c/FFD700:[PvPterra] Os teus ganhos ({0}) foram adicionados ao teu inventário!]" },
                    { "Inv_RestockSuccess", "[c/00FF00:[PvPterra] Os teus mantimentos foram reabastecidos!]" },

                    // --- Mensagens de Região e Estado da Partida ---
                    { "Reg_RespawnRestore", "[PvPterra] Reapareceste. O teu inventario original foi restaurado." },
                    { "Reg_SpectatorEliminated", "[PvPterra] Foste eliminado. Modo espectador ativado (Usa a Scrying Orb)." },
                    { "Reg_FleeKill", "[PvPterra] Foste morto por tentares fugir do combate!" },
                    { "Reg_LeaveRestore", "[PvPterra] Saíste da zona PvP ({0}). O teu inventário foi restaurado." },
                    { "Reg_LeaveNoRestore", "[PvPterra] Saíste da zona PvP ({0})." },
                    { "Reg_FFANeedFunds", "[PvPterra] Precisas de {0} para entrar no FFA." },
                    { "Reg_FFAEnter", "[PvPterra] Entraste no combate FFA!" },
                    { "Reg_WaitingPlayers", "[PvPterra] Esperando jogadores para {0} ({1}/{2})" },
                    { "Reg_KickNoFunds", "[PvPterra] Foste expulso da sala por não teres os {0} necessários." },
                    { "Reg_MatchAbortedFunds", "[PvPterra] Um jogador não tinha fundos suficientes. Voltando para a sala de espera..." },
                    { "Reg_MatchStarted", "[PvPterra] Vagas preenchidas ({0}/{0})! A partida {1} começou!" },
                    { "Reg_MatchEndWinner", "[c/FFD700:[PvPterra] Batalha terminada! Vencedores: Equipa {0} ({1})]" },
                    { "Reg_MatchEndTie", "[c/FFD700:[PvPterra] A batalha terminou em Empate!]" },
                    { "Reg_ResetTimer", "[PvPterra] Ativando sala de espera em 5 segundos..." },
                    { "Reg_TeamRestored", "[PvPterra] A tua equipa foi restaurada à força pela arena!" },
                    { "Reg_SpectatorInCourse", "[PvPterra] Há uma batalha em curso, entraste como espectador." },
                    { "Reg_SpectatorPref", "[PvPterra] Entraste em modo espectador (Preferência guardada)." },
                    { "Reg_TeamFull", "[PvPterra] Não podes entrar. A equipa {0} está cheia ou não participa." },
                    { "Reg_ArenaFull", "[PvPterra] Não podes entrar. Todas as vagas de combate para {0} estão preenchidas." },
                    { "Reg_InternalError", "[PvPterra] Erro interno ao ativar o teu PvP. Verifica a consola." },
                    { "Reg_StrictBuffs", "Os buffs desta região são rigorosos." },
                    { "Log_BuffError", "[PvPterra] Erro em EnforcePvPBuffs: {0}" },
                    { "Log_EnterRegionError", "[PvPterra] ERRO CRÍTICO em EnterPvPRegion: {0}\n{1}" },

                    // --- Ajuda de Comandos de Região ---
                    { "Cmd_Reg_Help_Exist", "Opções para regiões existentes:" },
                    { "Cmd_Reg_Help_Assign", "/pvpregion assign [nome] [modo] [recompensa?]" },
                    { "Cmd_Reg_Help_Remove", "/pvpregion remove [nome]" },
                    { "Cmd_Reg_Help_Redefine", "/pvpregion redefine [nome] [modo] [recompensa]" },
                    { "Cmd_Reg_Help_New", "Opções para criar regiões do zero:" },
                    { "Cmd_Reg_Help_Set", "/pvpregion set [1/2]" },
                    { "Cmd_Reg_Help_Define", "/pvpregion define [nome] [modo] [recompensa?]" },
                    { "Cmd_Reg_Help_Delete", "/pvpregion delete [nome]" },

                    // --- Respostas de Comandos de Região ---
                    { "Cmd_Reg_Assign_Syntax", "Uso: /pvpregion assign [nome] [modo] [recompensa?]" },
                    { "Cmd_Reg_NotExist", "A região '{0}' não existe. Crie-a primeiro ou use /pvpregion define." },
                    { "Cmd_Reg_Assigned", "[PvPterra] '{0}' atribuída. Modo: {1} | Recompensa: {2}." },

                    { "Cmd_Reg_Define_Syntax", "Uso: /pvpregion define [nome] [modo] [recompensa]" },
                    { "Cmd_Reg_Defined", "[PvPterra] Criada e definida. Modo: {0} | Recompensa: {1}." },

                    { "Cmd_Reg_Remove_Syntax", "Uso: /pvpregion remove [nome]" },
                    { "Cmd_Reg_Removed", "[PvPterra] Zona PvP removida de '{0}'. (A região física permanece intacta)." },
                    { "Cmd_Reg_NotAssigned", "A região '{0}' não estava atribuída como zona PvP." },

                    { "Cmd_Reg_Redefine_Syntax", "Uso: /pvpregion redefine [nome] [XvsX/FFA] [Recompensa]" },
                    { "Cmd_Reg_Redefine_Example", "Exemplo: /pvpregion redefine Arena 2vs2 5g" },
                    { "Cmd_Reg_Redefined", "[PvPterra] Região '{0}' atualizada -> Modo: {1} | Recompensa: {2}." },

                    { "Cmd_Reg_Set_Syntax", "Uso: /pvpregion set [1/2]" },

                    { "Cmd_Reg_Delete_Syntax", "Uso: /pvpregion delete [nome]" },
                    { "Cmd_Reg_Deleted", "[PvPterra] Região física '{0}' DESTRUÍDA do mapa e removida do PvP." },

                    { "Cmd_Reg_Unknown", "Subcomando não reconhecido. Digite /pvpregion para ver a ajuda." },
                }
            }
        };

        public static string GetString(string key, params object[] args)
        {
            if (_texts.ContainsKey(CurrentLang) && _texts[CurrentLang].ContainsKey(key))
            {
                string text = _texts[CurrentLang][key];
                return args.Length > 0 ? string.Format(text, args) : text;
            }

            if (_texts["en"].ContainsKey(key))
            {
                string text = _texts["es"][key];
                return args.Length > 0 ? string.Format(text, args) : text;
            }

            TShock.Log.ConsoleError($"[PvPterraUtils] Missing translation key: {key}");
            return $"[{key}]";
        }
    }
}