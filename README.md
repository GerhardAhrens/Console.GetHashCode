# GetHashCode() - Varianten

![NET](https://img.shields.io/badge/NET-8.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2022-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2025.0-yellow.svg)]

In C# wird die Methode GetHashCode() verwendet, um einen Hash-Code für ein Objekt zu erhalten. Sie ist eine wichtige Methode in der objektorientierten Programmierung, da sie beim effizienten Nachschlagen und Abrufen in Datenstrukturen wie Wörterbüchern, Hash-Tabellen und Sets hilft. Der Hash-Code ist ein numerischer Wert, der auf der Grundlage des Inhalts eines Objekts generiert wird und der zur effizienten Identifizierung von Objekten dient.

### Was ist GetHashCode()
Die Methode GetHashCode() ist ein Methode des Basisobjekttyps, in C#, was bedeutet, dass alle Klassen in C# diese Methode erben und in der Ableitung überschrieben werden kann. Sie gibt eine ganze Zahl zurück, die den Hash-Code des aktuellen Objekts darstellt. Der generierte Hash-Code basiert auf dem Inhalt der Felder und Eigenschaften des Objekts.

### Warum ist GetHashCode() wichtig?
GetHashCode() wird hauptsächlich in Szenarien verwendet, in denen Objekte auf Gleichheit verglichen oder in Hash-basierten Datenstrukturen gespeichert werden müssen. Indem wir einen Hash-Code für ein Objekt erhalten, können wir schnell feststellen, ob zwei Objekte potenziell gleich sind, ohne dass wir alle ihre individuellen Felder und Eigenschaften vergleichen müssen. Dies kann die Leistung bei der Arbeit mit großen Objektsammlungen erheblich verbessern.

### Wie funktioniert GetHashCode()?

Die Methode GetHashCode() berechnet den Hash-Code für ein Objekt, indem sie die Hash-Codes der einzelnen Felder und Eigenschaften kombiniert. C# bietet eine Implementierung von GetHashCode() für Wertetypen wie Integer, Floats und Strings. Für Referenztypen, wie benutzerdefinierte Klassen, gibt die Standardimplementierung von GetHashCode() die Speicheradresse des Objekts zurück.

Es ist jedoch gängige Praxis, GetHashCode() in benutzerdefinierten Klassen zu überschreiben, um eine sinnvollere und effizientere Implementierung bereitzustellen. Der Grund dafür ist, dass die Standardimplementierung zu Kollisionen führen kann, bei denen verschiedene Objekte denselben Hashcode erzeugen. Kollisionen können sich negativ auf die Leistung von Hash-basierten Datenstrukturen auswirken.

```csharp
public class Contact
{
    public string Name { get; set; }
    public int Age { get; set; }

    public override int GetHashCode()
    {
        unchecked //Überlauf ist in Ordnung, bzw. gewollt, einfach einpacken
        {
            int hash = 17;
            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + Age.GetHashCode();
            return hash;
        }
    }
}
```

### Nachteil der herkömmlichen Implementierung

Zum einen muß der umgabg mit **null** behandelt werden, zum anderen ist einer erweiterte Implementierung notwendig, wenn z.B. eine List<T> oder auch ein Dictionary<K,V> als Propertiy mit einbezogen werden soll.
Ein weiteres Problem kann sein, hier passende **Magic Numbers** einzusetzen um Kollisionen so gering wie möglich zu halen. Primzahlen sind dabei am besten geeignet.

Ich weiß nicht, wie es Ihnen geht, aber dieser Code sieht für mich furchtbar unübersichtlich aus. Zunächst einmal haben wir zwei verschiedene magische Zahlen: 17 und 23. Und warum? Zufälligerweise handelt es sich dabei um Primzahlen, was die Wahrscheinlichkeit von Kollisionen zwischen Hashes verringert (zwei ungleiche Objekte sollen unterschiedliche Hash-Codes haben, aber manchmal ist das aufgrund von Hash-Kollisionen nicht der Fall).

Außerdem gibt es das C#-Schlüsselwort unchecked, mit dem die Überlaufprüfung zur Verbesserung der Leistung gestoppt wird (so etwas sieht man nicht jeden Tag). Denken Sie daran, dass der Sinn der GetHashCode-Methode darin besteht, dass Dinge wie der Dictionary-Typ schnell Objekte abrufen können.

Ich persönlich wäre nicht in der Lage, mir jedes Mal, wenn ich GetHashCode implementieren muss, zu merken, wie man das macht, und es scheint, als könnte man durch einen Tippfehler sehr leicht Fehler einführen. 

## Implementierung von GetHashCode() unter NET Core

Mit NET Core 2.1 wurde die Struktur **HashCode** unter den Namespace *System* hinzugefügt.
Hiermit kann nun auf einfache Weise die Funktionalität für **GetHashCode()** ohne magische Zahlen und Prüfung des Speicherüberlauf *unchecked* erstellt werden.</br>


Bei diesem Beispiel werden alle Public Property zur Bildung des Object-Hashcode unter Verwendung der Klasse *HashCode* erstellt.
```csharp
public class ContactV2
{
    #region Properties
    public string Name { get; set; }
    public int Age { get; set; }
    #endregion Properties

    public override int GetHashCode()
    {
        int result = 0;
        var hash = new HashCode();

        try
        {
            PropertyInfo[] propInfo = this.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance);
            foreach (PropertyInfo propItem in propInfo)
            {
                hash.Add(propItem.GetValue(this, null));
            }

            result = hash.ToHashCode();
        }
        catch (Exception)
        {
            throw;
        }

        return result;
    }
}
```

</br>
In diesem Beispiel wird wie zuvor der Hashcode über die Klasse *HashCode* ermittelt. Die Erweiterung ist hier die Methode *CalculateHash* über die Lamda-Expression kann die Hashcode Erstellung auf einzelnen Properties begrenzt werden.</br>

```csharp
public class ContactV3
{
    #region Properties
    public string Name { get; set; }
    public int Age { get; set; }
    #endregion Properties

    public override int GetHashCode()
    {
        return CalculateHash<ContactV3>(x => x.Name, x => x.Age);
    }

    private int CalculateHash<T>(params Expression<Func<T, object>>[] expressions)
    {
        int result = 0;
        HashCode hash = new HashCode();
        Type type = typeof(T);

        try
        {
            foreach (var property in expressions)
            {
                string propertyName = ExpressionPropertyName.For<T>(property);
                object propertyValue = type.GetProperty(propertyName).GetValue(this);
                if (string.IsNullOrEmpty(propertyName) == false && propertyValue != null)
                {
                    hash.Add(propertyValue);
                }
            }

            result = hash.ToHashCode();
        }
        catch (Exception)
        {
            throw;
        }

        return result;
    }
```

