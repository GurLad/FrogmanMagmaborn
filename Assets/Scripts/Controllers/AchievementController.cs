using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementController
{
    public static void UnlockAchievement(string name)
    {
#if !UNITY_EDITOR && !MODDABLE_BUILD
#if STEAM_BUILD
        // TBA
#endif
        SavedData.Save("Achievements", name, 1);
#endif
    }

    public static bool HasAchievement(string name)
    {
#if !UNITY_EDITOR && !MODDABLE_BUILD
#if STEAM_BUILD
        // TBA
#endif
        return SavedData.Load("Achievements", name, 0) == 1;
#endif
        return false;
    }
}

/*
 * Achievement list:
 * - The Princess/Wizard/Scholar/Duke/Blacksmith/Rebel/Rogue - Join forces with Firbell/Xirveros/Kresla/Xeplogi/Memerian/Alfred/Fashima.
 * - Parts:
 *   - Let's get started, shall we? - Reach part 1 (monster).
 *   - Deal with the Devil - Reach part 2 (prisoner).
 *   - The Chosen One - Reach part 3 (champion).
 *   - We're in the endgame now - Reach part 4 (pawn).
 * - Bosses:
 *   - ??? - Defeat the Lich.
 *   - ??? - Defeat Fashima.
 *   - Impossible! - Defeat Lan.
 *   - ??? - Defeat Brens.
 *   - ??? - Defeat(?) Werse.
 *   - This is too easy - Defeat Bodder.
 *   - Crystal Overload - Defeat Bodder's final form.
 * - Endings:
 *   - ??? - Finish the game.
 *   - ??? - Finish the game on Insane.
 *   - Martyr - Reach Fashima's ending.
 *   - Eternal Torment - Reach Torment's ending.
 *   ? But the future refused to change - Support neither side in the endgame
 * - Conversation Chains:
 *   - Talk, talk - Finish the Talking Frog conversation chain.
 *   - Smiles will betray you~ - Finish the Xirveros' Past conversation chain.
 *   - ??? - Finish the Kresla & Alfred conversation chain.
 *   - Sore Loser - Defeat Lan 4 times, then reach him again.
 *   - Tragic Backstory - Find all flashback sequences.
 * - Misc.:
 *   - Good Pawn - Finish the tutorial.
 *   - ??? - Unlock all inclination changing upgrades.
 *   - Power! Unlimited Power! - Unlock all Torment powers.
 *   ? The best offense is a good defence - Finish the game using all defensive (blue) Torment powers (TBA: actually change all Torment powers so blue - defensive & red - offensive).
 *   ? Glass Cannons Galore - Finish the game using all offensive (red) Torment powers.
 *   
 * Internal names:
 * - Join forces: JoinX (ex. JoinFirbell)
 * - Part: PartX (ex. Part1)
 * - Defeat: DefeatX (ex. DefeatLich). For Bodder: DefeatBodder, DefeatSuperBodder.
 * - Endings: EndingX (ex. EndingFashima)
 * - Conversations: ConversationX (ex. ConversationTalkingFrog)
 * - Misc: MiscX (ex. MiscTutorialFinished)
 */ 