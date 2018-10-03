using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using coffeebucks;

public class coffeebucksScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable nextCustomer;
    public KMSelectable[] tabletButtons;
    public Renderer logo;
    public GameObject quirks;
    public KMSelectable[] quirkButtons;
    public Material[] quirkColours;
    public bool[] quirkStatus;
    public string[] quirkNames;
    public Coffee[] coffees;
    public AudioSource namesPlayer;
    public AudioClip[] namesAudio;
    public AudioClip[] welcomeAudio;
    public AudioClip[] thanksAudio;
    private int welcomeIndex = 0;

    public KMSelectable nameButton;
    public KMSelectable repeatName;
    public string[] nameOptions;
    public Color[] nameColors;
    public TextMesh displayedName;
    public TextMesh cupName;
    public KMSelectable nameLeft;
    public KMSelectable nameRight;
    private int startName = 0;
    private int selNameIndex = 0;

    public KMSelectable coffeeButton;
    [TextArea] public string[] coffeeOptions;
    public TextMesh displayedCoffee;
    public KMSelectable coffeeLeft;
    public KMSelectable coffeeRight;
    private int startCoffee = 0;

    public string customerName;
    public int[] allCustomerQuirks;
    public Color[] allCustomerColors;
    public int customerQuirk;
    public TextMesh[] preferences;
    public string[] selectedPreferences;
    [TextArea] public string[] preferenceOptions;
    private int modifier = 0;
    private int pressedButtonIndex = 0;
    [TextArea] public List<String> legalCoffees = new List<String>();
    private List<int> prefIndices = new List<int>();

    private float tipCount = 0.00f;
    public TextMesh tipCountText;
    public TextMesh penaltyText;

    private float currentTip = 9.99f;
    public TextMesh currentTipText;
    private float hintSubtract = 0.50f;

    private bool activeCustomer;
    private bool displayAll;
    private bool buttonPressed;
    private bool moduleSolved;
    private bool nameRepeated;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        nextCustomer.OnInteract += delegate () { OnNextCustomer(); return false; };
        nameButton.OnInteract += delegate () { OnNameButton(); return false; };
        nameLeft.OnInteract += delegate () { OnNameLeft(); return false; };
        nameRight.OnInteract += delegate () { OnNameRight(); return false; };
        repeatName.OnInteract += delegate () { OnRepeatName(); return false; };
        coffeeButton.OnInteract += delegate () { OnCoffeeButton(); return false; };
        coffeeLeft.OnInteract += delegate () { OnCoffeeLeft(); return false; };
        coffeeRight.OnInteract += delegate () { OnCoffeeRight(); return false; };
        foreach (KMSelectable iterator in tabletButtons)
        {
            KMSelectable pressedButton = iterator;
            iterator.OnInteract += delegate () { OnTabletButton(pressedButton); return false; };
        }
        foreach (KMSelectable iteratorQ in quirkButtons)
        {
            KMSelectable pressedButton = iteratorQ;
            iteratorQ.OnInteract += delegate () { OnQuirkButtons(pressedButton); return false; };
        }
    }

    void Start()
    {
        for(int i = 0; i<= 3; i++)
        {
            tabletButtons[i].gameObject.SetActive(false);
            preferences[i].text = "";
        }
        foreach(KMSelectable quirk in quirkButtons)
        {
            quirk.GetComponent<Renderer>().material = quirkColours[0];
        }
        nameRepeated = false;
        quirks.gameObject.SetActive(false);
        tipCountText.text = tipCount.ToString("F2");
        nextCustomer.gameObject.SetActive(true);
        logo.gameObject.SetActive(true);
        nameButton.gameObject.SetActive(false);
        nameLeft.gameObject.SetActive(false);
        repeatName.gameObject.SetActive(false);
        nameRight.gameObject.SetActive(false);
        coffeeButton.gameObject.SetActive(false);
        coffeeLeft.gameObject.SetActive(false);
        coffeeRight.gameObject.SetActive(false);
        hintSubtract = 0.50f;
        penaltyText.text = "Penalty for repeating\npreferences: $" + hintSubtract.ToString("F2");
        penaltyText.gameObject.SetActive(false);
        cupName.text = "";
        if(moduleSolved)
        {
            nextCustomer.gameObject.SetActive(false);
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Coffeebucks #{0}] You have ${1} in tips. Module disarmed.", moduleId, tipCount.ToString("F2"));
        }
        else
        {
            Debug.LogFormat("[Coffeebucks #{0}] Welcome to Coffeebucks! You currently have ${1} in tips.", moduleId, tipCount.ToString("F2"));
        }
    }


    IEnumerator tipCountDown()
    {
        currentTip = 9.99f;
        currentTipText.text = currentTip.ToString("F2");
        while(activeCustomer && currentTip >= 0.51f)
        {
            currentTip -= 0.01f;
            currentTipText.text = currentTip.ToString("F2");
            yield return new WaitForSeconds(0.1f);
        }
        if(currentTipText.text == "0.51")
        {
            currentTip = 0.50f;
            currentTipText.text = currentTip.ToString("F2");
        }
    }

    IEnumerator displayPreferences()
    {
        if(displayAll)
        {
            while(prefIndices.Count() < 4)
            {
                int prefIndex = UnityEngine.Random.Range(0,4);
                while(prefIndices.Contains(prefIndex))
                {
                    prefIndex = UnityEngine.Random.Range(0,4);
                }
                prefIndices.Add(prefIndex);
                yield return new WaitForSeconds(0.6f);
                preferences[prefIndex].text = selectedPreferences[prefIndex];
                if(prefIndices.Count() == 3)
                {
                    preferences[prefIndices[0]].text = "";
                }
                else if(prefIndices.Count() == 4)
                {
                    preferences[prefIndices[1]].text = "";
                }
            }
            yield return new WaitForSeconds(0.6f);
            preferences[prefIndices[2]].text = "";
            yield return new WaitForSeconds(0.6f);
            displayAll = false;
            for(int i = 0; i <= 3; i++)
            {
                preferences[i].text = "";
            }
            prefIndices.Clear();
        }
        else
        {
            preferences[pressedButtonIndex].text = selectedPreferences[pressedButtonIndex];
            yield return new WaitForSeconds(1f);
            for(int i = 0; i <= 3; i++)
            {
                preferences[i].text = "";
            }
        }
        buttonPressed = false;
    }

    void SetUpNameArray()
    {
        for(int i = 0; i <= 9; i++)
        {
            allCustomerQuirks[i] = UnityEngine.Random.Range(0,5);
            allCustomerColors[i] = nameColors[allCustomerQuirks[i]];
        }
        startName = UnityEngine.Random.Range(0,10);
        displayedName.text = nameOptions[startName];
        displayedName.color = allCustomerColors[startName];
    }

    void SetUpCoffeeArray()
    {
        startCoffee = UnityEngine.Random.Range(0,8);
        displayedCoffee.text = coffeeOptions[startCoffee];
    }

    public void OnNextCustomer()
    {
        if(moduleSolved)
        {
            return;
        }
        nextCustomer.AddInteractionPunch();
        nameButton.gameObject.SetActive(true);
        nameLeft.gameObject.SetActive(true);
        nameRight.gameObject.SetActive(true);
        repeatName.gameObject.SetActive(true);
        buttonPressed = true;
        displayAll = true;
        activeCustomer = true;
        StartCoroutine(tipCountDown());
        SetUpNameArray();
        SelectCustomerPreferences();
        nextCustomer.gameObject.SetActive(false);
        logo.gameObject.SetActive(false);
        for(int i = 0; i<= 3; i++)
        {
            tabletButtons[i].gameObject.SetActive(true);
        }
        StartCoroutine(displayPreferences());
        StartCoroutine(playAudio());
        Debug.LogFormat("[Coffeebucks #{0}] Your current customer is {1}. Their sugar-preference: {2}. The time of day: {3}. Their stress-level: {4}. Their preferred size: {5}. Their quirk: {6}.", moduleId, customerName, selectedPreferences[0].Replace("\n", " "), selectedPreferences[1], selectedPreferences[2], selectedPreferences[3], quirkNames[customerQuirk]);
        Debug.LogFormat("[Coffeebucks #{0}] {1}'s ideal coffees are: {2}.", moduleId, customerName, string.Join(", ", legalCoffees.Select((x) => x.Replace("\n", " ")).ToArray()));
    }

    void SelectCustomerPreferences()
    {
        modifier = 0;
        selNameIndex = UnityEngine.Random.Range(0,10);
        customerName = nameOptions[selNameIndex];
        customerQuirk = allCustomerQuirks[selNameIndex];
        for(int i = 0; i <= 3; i++)
        {
            int index = UnityEngine.Random.Range(0,4);
            selectedPreferences[i] = preferenceOptions[index + modifier];
            modifier += 4;
        }
        modifier = 0;
        foreach(Coffee option in coffees)
        {
            for(int i = 0; i <= 3; i++)
            {
                if(selectedPreferences[i] == option.coffeeQualities[i])
                {
                    option.matchingQualities++;
                }
            }
        }
        foreach(Coffee option in coffees)
        {
            if(option.matchingQualities == 4)
            {
                legalCoffees.Add(option.coffeeName);
            }
        }
        if(legalCoffees.Count > 0)
        {
            return;
        }
        else
        {
            foreach(Coffee option in coffees)
            {
                if(option.matchingQualities == 3)
                {
                    legalCoffees.Add(option.coffeeName);
                }
            }
        }
        if(legalCoffees.Count > 0)
        {
            return;
        }
        else
        {
            foreach(Coffee option in coffees)
            {
                if(option.matchingQualities == 2)
                {
                    legalCoffees.Add(option.coffeeName);
                }
            }
        }
        if(legalCoffees.Count > 0)
        {
            return;
        }
        else
        {
            foreach(Coffee option in coffees)
            {
                if(option.matchingQualities == 1)
                {
                    legalCoffees.Add(option.coffeeName);
                }
            }
        }
        if(legalCoffees.Count > 0)
        {
            return;
        }
        else
        {
            foreach(Coffee option in coffees)
            {
                if(option.matchingQualities == 0)
                {
                    legalCoffees.Add(option.coffeeName);
                }
            }
        }
    }

    IEnumerator playAudio()
    {
        welcomeIndex = UnityEngine.Random.Range(0,3);
        Audio.PlaySoundAtTransform(welcomeAudio[welcomeIndex].name, transform);
        yield return new WaitForSeconds(3.5f);
        namesPlayer.clip = namesAudio[selNameIndex];
        namesPlayer.Play();
    }

    public void OnNameButton()
    {
        nameButton.AddInteractionPunch();
        cupName.text = displayedName.text;
        if(cupName.text != customerName)
        {
            if(currentTip > 5f)
            {
                currentTip -= 5f;
                Debug.LogFormat("[Coffeebucks #{0}] Your customer's name is {1}. You have written {2}. Tip reduced by $5.", moduleId, customerName, cupName.text);
            }
            else
            {
                currentTip = 0f;
                Debug.LogFormat("[Coffeebucks #{0}] Your customer's name is {1}. You have written {2}. Tip reduced to $0.50.", moduleId, customerName, cupName.text);
            }
        }
        else
        {
            Debug.LogFormat("[Coffeebucks #{0}] Your customer's name is {1} and has been written correctly.", moduleId, customerName);
        }
        coffeeButton.gameObject.SetActive(true);
        coffeeLeft.gameObject.SetActive(true);
        coffeeRight.gameObject.SetActive(true);
        quirks.gameObject.SetActive(true);
        repeatName.gameObject.SetActive(false);
        nameButton.gameObject.SetActive(false);
        nameLeft.gameObject.SetActive(false);
        nameRight.gameObject.SetActive(false);
        penaltyText.gameObject.SetActive(true);
        SetUpCoffeeArray();
    }

    public void OnNameLeft()
    {
        nameLeft.AddInteractionPunch(.5f);
        startName = (startName + 9) % 10;
        displayedName.text = nameOptions[startName];
        displayedName.color = allCustomerColors[startName];
    }

    public void OnNameRight()
    {
        nameRight.AddInteractionPunch(.5f);
        startName = (startName + 1) % 10;
        displayedName.text = nameOptions[startName];
        displayedName.color = allCustomerColors[startName];
    }

    public void OnCoffeeButton()
    {
        coffeeButton.AddInteractionPunch();
        activeCustomer = false;
        if((customerQuirk == 0 && !quirkStatus[0] && !quirkStatus[1] && !quirkStatus[2] && !quirkStatus[3]) || (customerQuirk == 1 && quirkStatus[0] && !quirkStatus[1] && !quirkStatus[2] && !quirkStatus[3]) || (customerQuirk == 2 && !quirkStatus[0] && quirkStatus[1] && !quirkStatus[2] && !quirkStatus[3]) || (customerQuirk == 3 && !quirkStatus[0] && !quirkStatus[1] && quirkStatus[2] && !quirkStatus[3]) || (customerQuirk == 4 && !quirkStatus[0] && !quirkStatus[1] && !quirkStatus[2] && quirkStatus[3]))
        {
            Debug.LogFormat("[Coffeebucks #{0}] You have assigned the correct quirk (“{1}“).", moduleId, quirkNames[customerQuirk]);
        }
        else
        {
            if(currentTip > 2f)
            {
                currentTip -= 2f;
                currentTipText.text = currentTip.ToString("F2");
                Debug.LogFormat("[Coffeebucks #{0}] You have assigned the incorrect quirk or more than one quirk. Tip reduced by $2.", moduleId);
            }
            else
            {
                currentTip = 0.50f;
                currentTipText.text = currentTip.ToString("F2");
                Debug.LogFormat("[Coffeebucks #{0}] You have assigned the incorrect quirk or more than one quirk. Tip reduced to $0.50.", moduleId);
            }
        }

        if (legalCoffees.Any(answer => answer == displayedCoffee.text))
        {
            Debug.LogFormat("[Coffeebucks #{0}] You have given {1} a {2}. That is the correct coffee. Your tip total has risen by ${3}.", moduleId, customerName, displayedCoffee.text.Replace("\n", " "), currentTip.ToString("F2"));
            tipCount += currentTip;
            Audio.PlaySoundAtTransform(thanksAudio[welcomeIndex].name, transform);
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Coffeebucks #{0}] Strike! You have given {1} a {2}. That is not the correct coffee.", moduleId, customerName, displayedCoffee.text.Replace("\n", " "));
            currentTip = 0f;
            currentTipText.text = currentTip.ToString("F2");
        }
        legalCoffees.Clear();
        foreach(Coffee option in coffees)
        {
            option.matchingQualities = 0;
        }
        for(int i = 0; i <= 3; i++)
        {
            quirkStatus[i] = false;
            quirkButtons[i].GetComponent<Renderer>().material = quirkColours[0];
        }
        if(tipCount >= 10f)
        {
            moduleSolved = true;
        }
        Start();
    }

    public void OnCoffeeLeft()
    {
        coffeeLeft.AddInteractionPunch(.5f);
        startCoffee = (startCoffee + 7) % 8;
        displayedCoffee.text = coffeeOptions[startCoffee];
    }

    public void OnCoffeeRight()
    {
        coffeeRight.AddInteractionPunch(.5f);
        startCoffee = (startCoffee + 1) % 8;
        displayedCoffee.text = coffeeOptions[startCoffee];
    }

    public void OnRepeatName()
    {
        if(!nameRepeated)
        {
            nameRepeated = true;
            namesPlayer.clip = namesAudio[selNameIndex];
            namesPlayer.Play();
            repeatName.gameObject.SetActive(false);
            if(currentTip > 2f)
            {
                currentTip -= 2f;
                currentTipText.text = currentTip.ToString("F2");
                Debug.LogFormat("[Coffeebucks #{0}] Name has been repeated. Tip reduced by $2.", moduleId);
            }
            else
            {
                currentTip -= 0.5f;
                currentTipText.text = currentTip.ToString("F2");
                Debug.LogFormat("[Coffeebucks #{0}] Name has been repeated. Tip reduced to $0.50.", moduleId);
            }
        }
    }

    public void OnTabletButton(KMSelectable iterator)
    {
        if(buttonPressed)
        {
            return;
        }
        iterator.AddInteractionPunch(.5f);
        if(currentTip - hintSubtract <= 0.50)
        {
            currentTip = 0.50f;
            Debug.LogFormat("[Coffeebucks #{0}] Preference repeated. Tip reduced to $0.50.", moduleId);
        }
        else
        {
            currentTip -= hintSubtract;
            Debug.LogFormat("[Coffeebucks #{0}] Preference repeated. Tip reduced by ${1}.", moduleId, hintSubtract.ToString("F2"));
        }
        buttonPressed = true;
        for(int i = 0; i <= 3; i++)
        {
            if(iterator == tabletButtons[i])
            {
                pressedButtonIndex = i;
                StartCoroutine(displayPreferences());
            }
        }
        if(hintSubtract * 2 > 10)
        {
            hintSubtract = 8f;
        }
        else
        {
            hintSubtract = hintSubtract * 2;
        }
        penaltyText.text = "Penalty for repeating\npreferences: $" + hintSubtract.ToString("F2");
        currentTipText.text = currentTip.ToString("F2");
    }

    public void OnQuirkButtons(KMSelectable iteratorq)
    {
        if(buttonPressed)
        {
            return;
        }
        iteratorq.AddInteractionPunch(.5f);
        for(int i = 0; i <= 3; i++)
        {
            if(iteratorq == quirkButtons[i] && !quirkStatus[i])
            {
                quirkStatus[i] = true;
                quirkButtons[i].GetComponent<Renderer>().material = quirkColours[1];
                Debug.LogFormat("[Coffeebucks #{0}] “{1}“ quirk set to true.", moduleId, quirkNames[i+1]);
            }
            else if(iteratorq == quirkButtons[i] && quirkStatus[i])
            {
                quirkStatus[i] = false;
                quirkButtons[i].GetComponent<Renderer>().material = quirkColours[0];
                Debug.LogFormat("[Coffeebucks #{0}] “{1}“ quirk set to false.", moduleId, quirkNames[i+1]);
            }
        }
    }
}
