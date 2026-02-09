
- [х] Открытие и закрытие на клавишу клавы
- [х] Подсвечивать требование
- [x] Установить цвет обводки требуемого
- [x] Установить ширину обводки требуемого
- [ ] Отображение информации при наведении как окно или в фиксированном окне на выбор
- [ ] Отключить для этого проекта окно для лута, получаем через отправку
- [x] Исправить баг переноса из экипировки в экипировку
- [x] Исправить баг переноса из инвентаря в занятую экипировку
- [x] Исправить баг переноса из экипировки в занятую экипировку
- [x] Положить этот API в проект 
- [x] UserInterfaceRaycaster

#### GlobalBox.cs

* Пространство имён (Namespace): SkyClerik.Utils
* Описание: Это наш главный менеджер! Он управляет состояниями игры, а ещё сервисами сохранения и загрузки. 
* GlobalBox регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:
```
GlobalBox globalBox = ServiceProvider.Get<GlobalBox>();
```

Далее получаем желаемые сервисы через свойства:

```cs
// Даст доступ к сервису сохранения данных.
SaveService SaveService { get; }
// Даст доступ к сервису загрузки данных.
LoadService LoadService { get; }
```
    
```cs
// Поможет узнать текущие глобальные состояние игры, глобальные флаги, настройки сохранения.
GlobalGameProperty GlobalGameProperty { get; }

//[Header("Состояние игры")]

[Tooltip("Текущее состояние игры (например, в главном меню, в игре, на паузе).")]
public GameState CurrentGameState => _currentGameState;

[Tooltip("Установлено в 'true', если это новая игра. 'false' - если загруженная.")]
public bool IsNewGame => _isNewGame;

//[Header("Глобальные флаги")]
 
[Tooltip("Установлено в 'true', если игрок уже посмотрел вступительный ролик/обучение.")]
public bool HasSeenIntro => _hasSeenIntro;

[Tooltip("Установлено в 'true', если игрок не хочет видеть автоматические подсказки.")]
public bool HasInfomatron => _hasInfomatron;

[Tooltip("Текущий счет или очки игрока.")]
public int PlayerScore => _playerScore;
 
[Tooltip("Установлено в 'true', если игроку разрешено открыть окно крафта")]
public bool MakeCraftAccessible { get => _craftAccessible; set => _craftAccessible = value; }

//[Header("Сохранение")]
 
[Tooltip("Индекс текущего слота сохранения (например, 0, 1, 2).")]
public int CurrentSaveSlotIndex => _currentSaveSlotIndex;
```
 
---

#### GlobalItemStorage.cs
 
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Это наше глобальное хранилище для всех предметов и их префабов в игре. Тут можно найти все определения предметов и их визуальное представление.
* **GlobalItemStorage** регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:

```cs
GlobalItemStorage itemStorage = ServiceProvider.Get<GlobalItemStorage>();
```

Далее получаем желаемое хранилище через свойства:

```cs
// Даёт определение глобального хранилища всех данных предметов.
ItemsDataStorageDefinition GlobalItemsStorageDefinition { get; }

/// Возвращает клонированный ItemBaseDefinition по его itemID = (индексу в списке).
public ItemBaseDefinition GetClonedItem(int itemID)

/// Возвращает ItemBaseDefinition из внутреннего списка _baseDefinitions по указанному itemID = (индексу в списке).
/// Этот метод предоставляет прямой доступ к оригинальному объекту ItemBaseDefinition из списка,
/// что может быть полезно для чтения данных или для операций, не требующих клонирования.
/// Возвращает null, если индекс находится вне диапазона.
public ItemBaseDefinition GetOriginalItem(int itemID)
```

```cs
// Даёт определение хранилища префабов предметов.
ItemPrefabsStorageDefinition ItemPrefabsStorageDefinition { get; }

//Возвращает префаб для мира по его itemID (int). itemID должен совпадать с индексом в списке.
public GameObject GetPrefab(int itemID)
```

---

#### InventoryAPI.cs
  
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Фасад для управления UI инвентаря. Предоставляет упрощенный доступ к основным функциям взаимодействия с инвентарем, крафтом, сундуком, лутом и экипировкой.
* **InventoryAPI** регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:

```cs
InventoryAPI inventoryAPI = ServiceProvider.Get<InventoryAPI>();
```

```cs
// СОБЫТИЕ для получения колбека когда игрок нажал на предмет в инвентаре
 internal event Action<ItemBaseDefinition> OnItemGiven;

// Назначить свойства для обводки требуемого предмета (цвет и ширина)
public void SetGivinItemTracinColor(Color newColor, int width)

// Указывает, виден ли наш инвентарь.
public bool IsInventoryVisible { get => _itemsPage.IsInventoryVisible; set => _itemsPage.IsInventoryVisible = value; }

// Указывает, видна ли страница крафта.
public bool IsCraftVisible { get => _itemsPage.IsCraftVisible; set => _itemsPage.IsCraftVisible = value; }

// Указывает, видна ли страница сундука.
public bool IsCheastVisible { get => _itemsPage.IsCheastVisible; set => _itemsPage.IsCheastVisible = value; }

// Указывает, видна ли страница лута.
public bool IsLutVisible { get => _itemsPage.IsLutVisible; set => _itemsPage.IsLutVisible = value; }

// Указывает, видна ли страница экипировки.
 public bool IsEquipVisible { get => _itemsPage.IsEquipVisible; set => _itemsPage.IsEquipVisible = value; }

// Откроет инвентарь, чтобы выбрать предмет по его ID. Если предмета нет, инвентарь не откроется. tracing true если предмет должен подсвечиваться в инвентаре
public void OpenInventoryFromGiveItem(int itemID, bool tracing);

// Откроет инвентарь для выбора конкретного предмета. Если ссылка на предмет пустая, инвентарь не откроется. tracing true если предмет должен подсвечиваться в инвентаре
public void OpenInventoryGiveItem(ItemBaseDefinition item, bool tracing);

// Открывает обычный инвентарь и страницу крафта.
public void OpenInventoryAndCraft();

// Открывает обычный режим отображения инвентаря.
public void OpenInventoryNormal();

// Открывает страничку сундука.
public void OpenCheast();

// Открывает страничку лута.
public void OpenLut();

// Открывает страничку экипировки.
public void OpenEquip();

// Закрывает вообще все странички UI инвентаря.
public void CloseAll();

```

Пример для запроса предмета через itemID с возвратом по событию.

```cs
private void _bInventoryGive_clicked()
{
	if (_inventoryAPI.IsInventoryVisible)
	{
		_inventoryAPI.OnItemGiven -= OnItemGivenCallback;
		_inventoryAPI.CloseAll();
	}
	else
	{
		// открыть инвентарь для выбора предмета (отписки обязательные)
		_inventoryAPI.OnItemGiven += OnItemGivenCallback;
		_inventoryAPI.OpenInventoryFromGiveItem(itemID: 0);
	}
}

private void OnItemGivenCallback(ItemBaseDefinition itemBaseDefinition)
{
	if (itemBaseDefinition.ID == 0)
	{
		_inventoryAPI.OnItemGiven -= OnItemGivenCallback;
		Debug.Log($"выбран нужный предмет : {itemBaseDefinition.ID} - {itemBaseDefinition}");
		_inventoryAPI.CloseAll();
	}
}
```


#### LutContainer.cs : MonoBehaviour
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Объект, представляющий собой контейнер для лута. Хранит список предметов, которые могут быть переданы в другой контейнер  в автоматическом режиме или через окно выбора игроком.
* Указывается как ссылка в сцене и имеет два режима работы

```cs
// ВАРИАНТ 1 - передача лута из контейнера в инвентарь игрока через поиск свободного места от большего предмета к меньшему
[SerializeField]
private LutContainer _developLut;
 
 _developLut.TransferItemsToPlayerInventoryContainer();
```

```cs
// ВАРИАНТ 2 - Открыть окно с лутом для предоставления игроку выбора (в разработке)
[SerializeField]
private LutContainer _developLut;
 
 _developLut.OpenLutPage();
```

#### LutContainerWrappe.cs
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Объект, представляющий собой контейнер для лута. СОЗДАЕТ список предметов по переданному в конструктор списку itemID, которые могут быть переданы в другой контейнер в автоматическом режиме или через окно выбора игроком.

```cs
 public LutContainerWrapper(List<int> wrapperItemIndexes)
// ВАРИАНТ 1 - передача лута из контейнера в инвентарь игрока через поиск свободного места от большего предмета к меньшему
[SerializeField]
private LutContainer _developLut;
 
 _developLut.TransferItemsToPlayerInventoryContainer();
```

```cs
// ВАРИАНТ 2 - Открыть окно с лутом для предоставления игроку выбора (в разработке)
[SerializeField]
private LutContainer _developLut;
 
 _developLut.OpenLutPage();
```


#### InventoryContainersAPI.cs 
* Пространство имён (Namespace): SkyClerik.Inventory
* Описание: Фасад (API) для управления всеми контейнерами инвентаря игрока: основного инвентаря, контейнера крафта, сундука и лута. Предоставляет централизованный доступ к основным операциям с предметами для каждого типа контейнера.
* **InventoryContainersAPI** регистрируется в **ServiceProvider** автоматически (в Awake() и отменяет регистрацию в OnDestroy()).

Чтобы получить доступ к его сервисам или свойствам, нужно просто взять его из ServiceProvider так:

```cs
InventoryContainersAPI inventoryContainersAPI = ServiceProvider.Get<InventoryContainersAPI>();
```

```
// Я напишу только доступные на данный момент времени методы
// --- Методы для PlayerInventory ---
// Добавляет предметы из указанного контейнера лута в инвентарь.
public void AddItemsToPlayerInventory(ItemsList itemsList) => _playerInventory.AddItems(itemsList);

 // --- Методы для LutContainer ---
 // Отрабатывают в LutContainer и LutContainerWrappe не желательно пользовать на прямую
 // Добавляет предметы из указанного контейнера лута в окно лута.
 public void AddItemsToLutContainer(ItemsList itemsList) => _lutContainer.AddItems(itemsList);
```