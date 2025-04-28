# GetHashCode() - Varianten

![NET](https://img.shields.io/badge/NET-8.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2022-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2025.0-yellow.svg)]

In C# wird die Methode GetHashCode() verwendet, um einen Hash-Code f�r ein Objekt zu erhalten. Sie ist eine wichtige Methode in der objektorientierten Programmierung, da sie beim effizienten Nachschlagen und Abrufen in Datenstrukturen wie W�rterb�chern, Hash-Tabellen und Sets hilft. Der Hash-Code ist ein numerischer Wert, der auf der Grundlage des Inhalts eines Objekts generiert wird und der zur effizienten Identifizierung von Objekten dient.

### Was ist GetHashCode()
Die Methode GetHashCode() ist ein Methode des Basisobjekttyps, in C#, was bedeutet, dass alle Klassen in C# diese Methode erben und in der Ableitung �berschrieben werden kann. Sie gibt eine ganze Zahl zur�ck, die den Hash-Code des aktuellen Objekts darstellt. Der generierte Hash-Code basiert auf dem Inhalt der Felder und Eigenschaften des Objekts.

### Warum ist GetHashCode() wichtig?
GetHashCode() wird haupts�chlich in Szenarien verwendet, in denen Objekte auf Gleichheit verglichen oder in Hash-basierten Datenstrukturen gespeichert werden m�ssen. Indem wir einen Hash-Code f�r ein Objekt erhalten, k�nnen wir schnell feststellen, ob zwei Objekte potenziell gleich sind, ohne dass wir alle ihre individuellen Felder und Eigenschaften vergleichen m�ssen. Dies kann die Leistung bei der Arbeit mit gro�en Objektsammlungen erheblich verbessern.

### Wie funktioniert GetHashCode()?

Die Methode GetHashCode() berechnet den Hash-Code f�r ein Objekt, indem sie die Hash-Codes der einzelnen Felder und Eigenschaften kombiniert. C# bietet eine Implementierung von GetHashCode() f�r Wertetypen wie Integer, Floats und Strings. F�r Referenztypen, wie benutzerdefinierte Klassen, gibt die Standardimplementierung von GetHashCode() die Speicheradresse des Objekts zur�ck.

Es ist jedoch g�ngige Praxis, GetHashCode() in benutzerdefinierten Klassen zu �berschreiben, um eine sinnvollere und effizientere Implementierung bereitzustellen. Der Grund daf�r ist, dass die Standardimplementierung zu Kollisionen f�hren kann, bei denen verschiedene Objekte denselben Hashcode erzeugen. Kollisionen k�nnen sich negativ auf die Leistung von Hash-basierten Datenstrukturen auswirken.

```csharp
public class Contact
{
    public string Name { get; set; }
    public int Age { get; set; }

    public override int GetHashCode()
    {
        unchecked //�berlauf ist in Ordnung, bzw. gewollt, einfach einpacken
        {
            int hash = 17;
            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + Age.GetHashCode();
            return hash;
        }
    }
}
```

### Nachteil der herk�mmlichen Implementierung

Zum einen mu� der umgabg mit **null** behandelt werden, zum anderen ist einer erweiterte Implementierung notwendig, wenn z.B. eine List<T> oder auch ein Dictionary<K,V> als Propertiy mit einbezogen werden soll.
Ein weiteres Problem kann sein, hier passende **Magic Numbers** einzusetzen um Kollisionen so gering wie m�glich zu halen. Primzahlen sind dabei am besten geeignet.

Ich wei� nicht, wie es Ihnen geht, aber dieser Code sieht f�r mich furchtbar un�bersichtlich aus. Zun�chst einmal haben wir zwei verschiedene magische Zahlen: 17 und 23. Und warum? Zuf�lligerweise handelt es sich dabei um Primzahlen, was die Wahrscheinlichkeit von Kollisionen zwischen Hashes verringert (zwei ungleiche Objekte sollen unterschiedliche Hash-Codes haben, aber manchmal ist das aufgrund von Hash-Kollisionen nicht der Fall).

Au�erdem gibt es das C#-Schl�sselwort unchecked, mit dem die �berlaufpr�fung zur Verbesserung der Leistung gestoppt wird (so etwas sieht man nicht jeden Tag). Denken Sie daran, dass der Sinn der GetHashCode-Methode darin besteht, dass Dinge wie der Dictionary-Typ schnell Objekte abrufen k�nnen.

Ich pers�nlich w�re nicht in der Lage, mir jedes Mal, wenn ich GetHashCode implementieren muss, zu merken, wie man das macht, und es scheint, als k�nnte man durch einen Tippfehler sehr leicht Fehler einf�hren. 

## Implementierung von GetHashCode() unter NET Core

Mit NET Core 2.1 wurde die Struktur **HashCode** unter den Namespace *System* hinzugef�gt.
Hiermit kann nun auf einfache Weise die Funktionalit�t f�r **GetHashCode()** ohne magische Zahlen und Pr�fung des Speicher�berlauf *unchecked* erstellt werden.</br>


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
In diesem Beispiel wird wie zuvor der Hashcode �ber die Klasse *HashCode* ermittelt. Die Erweiterung ist hier die Methode *CalculateHash* �ber die Lamda-Expression kann die Hashcode Erstellung auf einzelnen Properties begrenzt werden.</br>

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

