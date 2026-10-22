public static class ChainReleaseValidation
{
    public static void Validate() {
        ChainUiValidation.Validate();
        MemoryExpansionValidation.Validate();
        for(int i=0;i<DeckCatalog.Count;i++) {
            var sprite=DeckCatalog.Mnemonic(i);
            if(sprite==null || sprite.texture.width>512 || sprite.texture.height>512)
                throw new System.Exception("Missing/oversized mnemonic sprite "+i);
        }
        UnityEngine.Debug.Log("CHAIN_RELEASE_VALIDATION_PASSED");
    }
}
