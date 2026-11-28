from pathlib import Path
import argparse,uuid
parser=argparse.ArgumentParser(description='Run only after Idriz chooses the pair identity rule.')
parser.add_argument('--pair-rule',choices=['associated','identical'],required=True)
args=parser.parse_args()
def change(path,before,after):
    p=Path(path);s=p.read_text(encoding='utf-8-sig');assert s.count(before)==1,(path,before[:80],s.count(before));p.write_text(s.replace(before,after),encoding='utf-8',newline='')
text=Path(__file__).with_name('FlashingPairsGame.cs.template').read_text(encoding='utf-8')
text=text.replace('new SagaPairsBoard(overlay.transform,state.Sequence,Quit','new SagaPairsBoard(overlay.transform,__DISPLAY_SEQUENCE__,Quit')
rules={
 'associated':{'__INTRO_RULES__':'"Remember the two different pictures in each numbered pair.\\nThen find those pairs."','__FACE_SELECTION__':'pool.Take(Pairs(level)*2).ToArray()','__STUDY_LABELS__':'board.LabelStudyPairs();','__SHUFFLE_RECALL__':'true','__DISPLAY_SEQUENCE__':'state.Sequence'},
 'identical':{'__INTRO_RULES__':'"Remember where the matching pictures are.\\nThen uncover the identical pairs."','__FACE_SELECTION__':'pool.Take(Pairs(level)).SelectMany(id=>new[]{id,id}).ToArray()','__STUDY_LABELS__':'','__SHUFFLE_RECALL__':'false','__DISPLAY_SEQUENCE__':'state.Sequence.OrderBy(id=>UnityEngine.Random.value).ToArray()'}
}
for key,value in rules[args.pair_rule].items():assert key in text; text=text.replace(key,value)
assert '__' not in text
text=text.replace('    float remaining;','    float remaining,lastTick;')
text=text.replace('remaining=StudySeconds(level);board.ShowClock','remaining=StudySeconds(level);lastTick=Time.realtimeSinceStartup;board.ShowClock')
text=text.replace('var content=result.transform.GetChild(0);UnityAction replay=()=>UseResult(()=>StartLevel(level)),back=()=>UseResult(ShowLevels);','var content=result.transform.GetChild(0);var shown=result;UnityAction replay=()=>UseResult(shown,()=>StartLevel(level)),back=()=>UseResult(shown,ShowLevels);')
text=text.replace('()=>UseResult(()=>StartLevel(level+1))','()=>UseResult(shown,()=>StartLevel(level+1))')
text=text.replace('void UseResult(UnityAction action){if(resultConsumed || owner!=GameManager.AchievementUserId)return;', 'void UseResult(GameObject shown,UnityAction action){if(shown!=result || resultConsumed || owner!=GameManager.AchievementUserId)return;')
text=text.replace('Tick(Time.unscaledDeltaTime);','float now=Time.realtimeSinceStartup,elapsed=Mathf.Max(0,now-lastTick);lastTick=now;Tick(elapsed);')
text=text.replace('backgrounded=paused;','backgrounded=paused;lastTick=Time.realtimeSinceStartup;')
p=Path('frontend/Assets/Scripts/GamesScripts/General/FlashingPairsGame.cs');assert not p.exists();p.write_text(text,encoding='utf-8')
p.with_suffix('.cs.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nMonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n',encoding='utf-8')
p='frontend/Assets/Scripts/views/TrainingHubLayout.cs'
change(p,'        if(!memory.Contains(says))memory.Add(says);', '''        if(!memory.Contains(says))memory.Add(says);
        var flashing=screen.Find("Flashing Pairs preview") as RectTransform;
        if(flashing==null){var go=ChainGame.Panel(screen,"Flashing Pairs preview",Vector2.zero,Vector2.one,FlashingPairsGame.Tone);flashing=(RectTransform)go.transform;go.AddComponent<Button>().onClick.AddListener(()=>FlashingPairsGame.Open(screen));}
        if(!memory.Contains(flashing))memory.Add(flashing);''')
change(p,'"CHAIN","SAYS"};','"CHAIN","SAYS","FLASHING\\nPAIRS"};')
change(p,'new Color32(106,28,225,255)};', 'new Color32(106,28,225,255),FlashingPairsGame.Tone};')
change(p,'TrainingGameSymbol.Draw((RectTransform)art.transform,index,Color.white,false);','''TrainingGameSymbol.Draw((RectTransform)art.transform,index==5?0:index,Color.white,false);
        if(index==5){var bolt=new GameObject("Existing Flash symbol",typeof(RectTransform),typeof(CanvasRenderer));bolt.transform.SetParent(art.transform,false);var rect=(RectTransform)bolt.transform;rect.anchorMin=new Vector2(.65f,.60f);rect.anchorMax=new Vector2(1,1);rect.offsetMin=rect.offsetMax=Vector2.zero;var graphic=bolt.AddComponent<TrainingFlashSymbol>();graphic.color=Color.white;graphic.raycastTarget=false;}''')
change(p,'new Vector2(.95f,.32f),46,Color.white)', 'new Vector2(.95f,.32f),index==5?30:46,Color.white)')
print('Flashing Pairs installed in Training with explicit rule:',args.pair_rule)
