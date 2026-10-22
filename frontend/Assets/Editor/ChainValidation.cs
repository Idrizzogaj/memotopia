using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Assets.Script.Constants;

public static class ChainValidation
{
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    public static void ValidateLogic() {
        Check(ChainState.CountForLevel(1)==3 && ChainState.CountForLevel(20)==52,"Chain endpoints");
        for(int i=2;i<=20;i++)Check(ChainState.CountForLevel(i)>ChainState.CountForLevel(i-1),"Chain level progression");
        var state=new ChainState(new[]{9,5,17});
        Check(!state.Choose(5) && state.Hearts==4 && state.Position==0,"Wrong choice must lose exactly one heart");
        Check(state.Choose(9) && state.Position==1,"Correct choice");
        Check(!state.Choose(9) && state.Hearts==4,"Already removed cards cannot cost a heart");
        Check(state.Choose(5) && state.Choose(17) && state.Won,"Ordered win");
        Check(!state.Choose(3) && state.Hearts==4,"Finished rounds immutable");
        state=new ChainState(new[]{1,2,3});for(int i=0;i<5;i++)state.Choose(3);
        Check(state.Hearts==0 && state.Finished && !state.Won && !state.Choose(1),"Five mistakes lose");
        var athlete=new ChainState(new[]{1,2,3},1);Check(athlete.Hearts==1 && !athlete.Choose(3) && athlete.Finished && !athlete.Won,"One-heart Athlete must fail on first mistake");
        int index=0,taken;Check(StudySteps.Take(ref index,3,1,out taken)&&taken==0&&index==1,"Study start");
        StudySteps.Take(ref index,3,1,out taken);StudySteps.Take(ref index,3,1,out taken);
        Check(!StudySteps.Take(ref index,3,1,out taken),"Study must finish");
        index=0;for(int i=0;i<10;i++)Check(StudySteps.Take(ref index,20,2,out taken)&&taken==i*2,"Pairs next regression");
        Check(!StudySteps.Take(ref index,20,2,out taken),"Pairs should show exactly 10 pairs");
    }
    public static void Validate() {
        ValidateLogic();
        var oldUser=UserConstants.s_user;bool oldChallenge=ChallengeConstants.s_isChallenge;
        UserConstants.s_user=new Assets.Script.Models.User{id=1999999992};
        string[] keys={"chain.completed.v1.1999999992","content.choice.v1.1999999992","memory.settings.v1.1999999992.deckMode"};
        var had=keys.Select(PlayerPrefs.HasKey).ToArray();var prior=keys.Select(k=>PlayerPrefs.GetInt(k)).ToArray();
        try {
            PlayerPrefs.DeleteKey(keys[0]);ChallengeConstants.s_isChallenge=false;
            ChainProgress.Complete(20);Check(!ChainProgress.All,"Last level alone cannot earn all-level badge");
            for(int i=1;i<20;i++)ChainProgress.Complete(i);
            Check(ChainProgress.All,"All 20 completed");
            UserConstants.s_user=new Assets.Script.Models.User{id=1999999993};Check(!ChainProgress.All,"Account leakage");
            UserConstants.s_user=new Assets.Script.Models.User{id=1999999992};
            CardFaces.Selected=CardFaces.Choice.Deck;
            foreach(DeckDisplay mode in Enum.GetValues(typeof(DeckDisplay))) {
                MemorySettings.DeckMode=mode;CardFaces.BeginSession();Check(CardFaces.ContentCount==52,"Deck scope");
                int cards=0,words=0,images=0;
                for(int id=1;id<=52;id++) {
                    var kind=CardFaces.KindOf(id);if(kind==CardKind.PlayingCard)cards++;else if(kind==CardKind.Word)words++;else if(kind==CardKind.Image){images++;Check(CardFaces.Face(id)!=null,"Missing deck mnemonic");}else throw new Exception("Generic content leaked into deck");
                    if(id<4)Check(CardFaces.Face(id)!=null && CardFaces.SmallFace(id)!=null,"Deck render");
                }
                if(mode==DeckDisplay.Cards)Check(cards==52,"Cards mode");
                if(mode==DeckDisplay.Images)for(int id=1;id<=52;id++)Check(CardFaces.Face(id)==DeckCatalog.Mnemonic(id-1),"Generic picture substituted for deck illustration");
                if(mode==DeckDisplay.Words)Check(words==52,"Deck word mode");
                if(mode==DeckDisplay.Images || mode==DeckDisplay.Decode)Check(images==52,"Deck mnemonic mode");
                if(mode==DeckDisplay.Random)Check(cards>0 && words>0 && images>0,"Deck mix");
                var frozen=CardFaces.KindOf(1);MemorySettings.DeckMode=DeckDisplay.Cards;Check(CardFaces.KindOf(1)==frozen,"Mid-round presentation drift");
            }
            Check(ExtraAchievements.IsExtra("chain-first")&&ExtraAchievements.IsExtra("chain-all"),"Chain badges");
        } finally {
            CardFaces.EndSession();for(int i=0;i<keys.Length;i++){if(had[i])PlayerPrefs.SetInt(keys[i],prior[i]);else PlayerPrefs.DeleteKey(keys[i]);}
            UserConstants.s_user=oldUser;ChallengeConstants.s_isChallenge=oldChallenge;PlayerPrefs.Save();
        }
        Debug.Log("CHAIN_VALIDATION_PASSED: ordered recall, 5 hearts, immutable end state, 20 levels, account isolation, 52-only deck modes, Pairs shared steps.");
    }
}
