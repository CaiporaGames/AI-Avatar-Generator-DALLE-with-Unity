using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using OpenAI;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class PlayerAvatarGenerator : MonoBehaviour
{
    [TextArea]
    [SerializeField] string headerPrompt;

    OpenAIApi api = new OpenAIApi();
    VisualElement root;
    Image avatarImage;
    DropdownField genderDropdown;
    DropdownField raceDropdown;
    DropdownField classFieldDropdown;
    Slider ageSlider;
    TextField personalityText;
    Button submitButton;

    string avatarGender = string.Empty;
    string avatarRace = string.Empty;
    string avatarClassField = string.Empty;
    string avatarAge = string.Empty;
    string avatarPersonality = string.Empty;

  
    private void OnEnable()
    {
        //Get the root element from the UI
        root = GetComponent<UIDocument>().rootVisualElement;
        avatarImage = root.Q<Image>(className:"avatarImage");

        genderDropdown = root.Q<DropdownField>(className:"genderDropdown");
        raceDropdown = root.Q<DropdownField>(className: "raceDropdown");
        classFieldDropdown = root.Q<DropdownField>(className: "classFieldDropdown");

        // Set the gender choices
        List<string> genderOptions = new List<string> { "Male", "Female" };
        genderDropdown.choices = genderOptions;
        genderDropdown.value = genderOptions[0];

        // Set the race choices
        List<string> raceOptions = new List<string> { "Human", "Orc", "Elf", "Dwarf", "Monster" };
        raceDropdown.choices = raceOptions;
        raceDropdown.value = raceOptions[0];

        // Set the classField choices
        List<string> classFieldOptions = new List<string> { "Warrior", "Healer", "Assassin", "Mage", "Ninja" };
        classFieldDropdown.choices = classFieldOptions;
        classFieldDropdown.value = classFieldOptions[0];

        genderDropdown.RegisterValueChangedCallback(OnGenderDropdownChanged);
        raceDropdown.RegisterValueChangedCallback(OnRaceDropdownChanged);
        classFieldDropdown.RegisterValueChangedCallback(OnclassFieldDropdownChanged);


        ageSlider = root.Q<Slider>(className:"ageSlider");

        // Set initial value of the slider
        ageSlider.value = 10;
        ageSlider.label = "Age: " + ageSlider.value;
        // Subscribe to the slider value change event
        ageSlider.RegisterValueChangedCallback(evt =>
        {
            avatarAge = evt.newValue.ToString();
            ageSlider.label = "Age: " + evt.newValue; // Update the slider's internal label text
        });

        personalityText = root.Q<TextField>(className: "personalityText");

        personalityText.RegisterValueChangedCallback(OnPersonalityChanged);

        submitButton = root.Q<Button>(className: "generateAvatarButton");
        submitButton.RegisterCallback<ClickEvent>(HandleSubmit);
    }

    private void OnPersonalityChanged(ChangeEvent<string> evt)
    {
        avatarPersonality = evt.newValue;
    }

    private void OnGenderDropdownChanged(ChangeEvent<string> evt)
    {
        avatarGender = evt.newValue;
    }

    private void OnRaceDropdownChanged(ChangeEvent<string> evt)
    {
        avatarRace = evt.newValue;
    }

    private void OnclassFieldDropdownChanged(ChangeEvent<string> evt)
    {
        avatarClassField = evt.newValue;
    }

    async void GenerateImage()
    {
        var request = new CreateImageRequest
        {
            Prompt = ConstructPrompt(),
            Size = ImageSize.Size256
        };

        var response = await api.CreateImage(request);

        if (response.Data.Count == 0 || response.Data == null)
        {
            Debug.Log("The prompt does not work.");
            return;
        }

        using (var webReq = new UnityWebRequest(response.Data[0].Url))
        {
            webReq.downloadHandler = new DownloadHandlerBuffer();
            webReq.SetRequestHeader("Access-Control-Allow-Origin", "");
            webReq.SendWebRequest();

            while (!webReq.isDone)
            {
                await Task.Yield();//Wait for a frame and repeat till done
            }

            Texture2D avatarTexture = new Texture2D(2,2);
            avatarTexture.LoadImage(webReq.downloadHandler.data);
            avatarImage.style.backgroundImage = new StyleBackground(avatarTexture);
        }
    }

    string ConstructPrompt()
    {
        string prompt = headerPrompt;
        prompt += $"\n Gender: { avatarGender }";
        prompt += $"\n Race: { avatarRace }";
        prompt += $"\n ClassField: { avatarClassField }";
        prompt += $"\n Age: {avatarAge}";
        prompt += $"\n Personality: {avatarPersonality}";
        Debug.Log(prompt);
        return prompt;
    }

    void HandleSubmit(ClickEvent evt)
    {
        GenerateImage();
    }
}
