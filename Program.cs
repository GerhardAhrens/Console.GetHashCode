//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lifeprojects.de">
//     Class: Program
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>28.04.2025 09:50:21</date>
//
// <summary>
// Konsolen Applikation mit Menü
// </summary>
//-----------------------------------------------------------------------

namespace Console.GetHashCode
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;

    public class Program
    {
        private static void Main(string[] args)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("1. bisherige Standard Lösung");
                Console.WriteLine("2. Klasse HashCode");
                Console.WriteLine("3. Klasse HashCode, Generic Expression");
                Console.WriteLine("X. Beenden");

                Console.WriteLine("Wählen Sie einen Menüpunkt oder 'x' für beenden");
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.X)
                {
                    Environment.Exit(0);
                }
                else
                {
                    if (key == ConsoleKey.D1)
                    {
                        MenuPoint1();
                    }
                    else if (key == ConsoleKey.D2)
                    {
                        MenuPoint2();
                    }
                    else if (key == ConsoleKey.D3)
                    {
                        MenuPoint3();
                    }
                }
            }
            while (true);
        }

        private static void MenuPoint1()
        {
            Console.Clear();

            ContactV1 contact1 = new ContactV1();
            contact1.Name = "Gerhard";
            contact1.Age = 64;
            int hashCode1 = contact1.GetHashCode();

            ContactV1 contact2 = new ContactV1();
            contact2.Name = "Gerhard";
            contact2.Age = 64;
            int hashCode2 = contact2.GetHashCode();

            if (hashCode1 == hashCode2)
            {
                Console.WriteLine($"HashCode von 'ContactV1' = {hashCode1} und 'ContactV2' = {hashCode2} sind gleich.");
            }

            Console.WriteLine("Mit einer beliebigen Taste zurück zum Menü!");
            Console.ReadKey();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

            ContactV2 contact1 = new ContactV2();
            contact1.Name = "Gerhard";
            contact1.Age = 64;
            int hashCode1 = contact1.GetHashCode();

            ContactV2 contact2 = new ContactV2();
            contact2.Name = "Gerhard";
            contact2.Age = 64;
            int hashCode2 = contact2.GetHashCode();

            if (hashCode1 == hashCode2)
            {
                Console.WriteLine($"HashCode von 'ContactV1' = {hashCode1} und 'ContactV2' = {hashCode2} sind gleich.");
            }

            Console.WriteLine("Mit einer beliebigen Taste zurück zum Menü!");
            Console.ReadKey();
        }

        private static void MenuPoint3()
        {
            Console.Clear();

            ContactV3 contact1 = new ContactV3();
            contact1.Name = "Gerhard";
            contact1.Age = 64;
            int hashCode1 = contact1.GetHashCode();

            ContactV3 contact2 = new ContactV3();
            contact2.Name = "Gerhard";
            contact2.Age = 64;
            int hashCode2 = contact2.GetHashCode();

            if (hashCode1 == hashCode2)
            {
                Console.WriteLine($"HashCode von 'ContactV1' = {hashCode1} und 'ContactV2' = {hashCode2} sind gleich.");
            }

            Console.WriteLine("Mit einer beliebigen Taste zurück zum Menü!");
            Console.ReadKey();
        }
    }

    public class ContactV1
    {
        #region Properties
        public string Name { get; set; }
        public int Age { get; set; }
        #endregion Properties

        public override int GetHashCode()
        {
            unchecked //Überlauf ist in Ordnung, bzw. gewollt, einfach einpacken
            {
                int hash = 17;
                hash = hash * 23 + this.Name.GetHashCode();
                hash = hash * 23 + this.Age.GetHashCode();
                return hash;
            }
        }
    }

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
    }
}
