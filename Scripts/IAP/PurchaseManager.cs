using UnityEngine;
using UnityEngine.Purchasing;

public class PurchaseManager : MonoBehaviour, IStoreListener
{
    private static IStoreController _storeController;
    private static IExtensionProvider _storeExtensionProvider;

    private static readonly string PRODUCT_GOLD_1000 = "test_gold_1000";
    private static readonly string PRODUCT_GOLD_2000 = "test_gold_2000";
    private static readonly string PRODUCT_REMOVE_ADS = "test_remove_ads";

    private void Start()
    {
        Debug.Log("[IAP] Start 호출됨");

        if (_storeController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        Debug.Log("[IAP] InitializePurchasing 호출됨");

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(PRODUCT_GOLD_1000, ProductType.Consumable);
        builder.AddProduct(PRODUCT_GOLD_2000, ProductType.Consumable);
        builder.AddProduct(PRODUCT_REMOVE_ADS, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    //  초기화 여부 확인용 메서드
    private bool IsInitialized()
    {
        return _storeController != null && _storeExtensionProvider != null;
    }

    public void BuyGold1000()
    {
        if (!IsInitialized())
        {
            Debug.LogWarning("[IAP] 초기화 전에는 구매할 수 없습니다.");
            return;
        }

        BuyProductID(PRODUCT_GOLD_1000);
    }

    public void BuyGold2000()
    {
        if (!IsInitialized())
        {
            Debug.LogWarning("[IAP] 초기화 전에는 구매할 수 없습니다.");
            return;
        }

        BuyProductID(PRODUCT_GOLD_2000);
    }

    public void BuyRemoveads()
    {
        if(!IsInitialized())
        {
            Debug.LogWarning("[IAP] 초기화 전에는 구매할 수 없습니다.");
            return;
        }
        
        BuyProductID(PRODUCT_REMOVE_ADS);
    }

    private void BuyProductID(string productId)
    {
        Debug.Log($"[IAP] BuyProductID 호출됨: {productId}");

        Product product = _storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"[IAP] 구매 시도: {productId}");
            _storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning($"[IAP] 구매 불가 또는 Product 없음: {productId}");
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"[IAP] ProcessPurchase 호출됨 - 구매된 상품: {args.purchasedProduct.definition.id}");

        switch (args.purchasedProduct.definition.id)
        {
            case "test_gold_1000":
                Debug.Log("[IAP] 골드 1000 지급");
                // TODO: 골드 1000 지급 처리
                break;

            case "test_gold_2000":
                Debug.Log("[IAP] 골드 2000 지급");
                // TODO: 골드 2000 지급 처리
                break;
            
            case "test_remove_ads":
                Debug.Log("[IAP] 광고 제거 상품 구매 완료");
                Managers.SaveLoad.SaveData.isAdRemoved = true;
                Managers.SaveLoad.Save(); // 실제 저장
                break;

            default:
                Debug.LogWarning($"[IAP] 알 수 없는 상품 ID: {args.purchasedProduct.definition.id}");
                break;
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogError($"[IAP] 구매 실패 - 상품: {product.definition.id}, 이유: {reason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAP] OnInitialized 호출됨 초기화 성공");
        _storeController = controller;
        _storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAP] 초기화 실패 - 이유: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAP] 초기화 실패 - 이유: {error}, 메시지: {message}");
    }

    #region 광고 제거 상태 확인 및 설정 (테스트용)
    public bool IsAdRemoved => Managers.SaveLoad.SaveData.isAdRemoved;

    public void SetTestRemoveAds(bool value)
    {
        Managers.SaveLoad.SaveData.isAdRemoved = value;
        Managers.SaveLoad.Save();
    }
    #endregion
}
