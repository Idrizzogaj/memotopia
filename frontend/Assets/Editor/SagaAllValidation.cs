using System;
using UnityEngine;

public static class SagaAllValidation
{
    public static void Validate(){
        if(Application.productName!="MemotopiaSagaValidation")throw new InvalidOperationException("Use the isolated validation project.");
        SagaValidation.Validate();
        SagaFocusValidation.Validate();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetFullPath("../ChainReuseFix"));
        FocusNativeValidation.Review();
        Debug.Log("SAGA_ALL_VALIDATION_PASSED:240stops,Memory/Focus goals and bosses,unchanged native Focus.");
    }
}
