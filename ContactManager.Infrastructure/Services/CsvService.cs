using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Models;
using ContactManager.Infrastructure.Configuration;
using CsvHelper;
using CsvHelper.Configuration;

namespace ContactManager.Infrastructure.Services
{
    public class CsvService : ICsvService
    {
        public IEnumerable<Contact> ParseContacts(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            var config = CsvConfigurationHelper.GetDefaultConfiguration();

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<ContactCsvMap>();

            var records = new List<ContactCsvRecord>();
            int rowCount = 0;
            
            try
            {
                // Читаем записи по одной для лучшей обработки ошибок
                while (csv.Read())
                {
                    rowCount++;
                    try
                    {
                        var record = csv.GetRecord<ContactCsvRecord>();
                        if (record != null)
                        {
                            records.Add(record);
                            Console.WriteLine($"Successfully parsed row {rowCount}: {record.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing row {rowCount}: {ex.Message}");
                        // Пропускаем проблемные строки и продолжаем
                        continue;
                    }
                }
                Console.WriteLine($"Total rows processed: {rowCount}, Successfully parsed: {records.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CSV parsing error: {ex.Message}");
                throw new Exception($"Ошибка при чтении CSV файла: {ex.Message}");
            }
            
            // Преобразуем в Contact объекты с обработкой ошибок
            var contacts = new List<Contact>();
            int conversionCount = 0;
            foreach (var record in records)
            {
                try
                {
                    Console.WriteLine($"Converting record: {record.Name}, DOB: {record.DateOfBirth}, Married: {record.Married}, Phone: {record.Phone}, Salary: {record.Salary}");
                    
                    // Проверяем и преобразуем дату с поддержкой различных форматов
                    DateTime dateOfBirth = default;
                    bool dateParsed = false;
                    
                    // Пробуем различные форматы дат
                    string[] dateFormats = { "yyyy-MM-dd", "dd.MM.yyyy", "MM/dd/yyyy", "dd/MM/yyyy" };
                    foreach (var format in dateFormats)
                    {
                        if (DateTime.TryParseExact(record.DateOfBirth, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfBirth))
                        {
                            dateParsed = true;
                            break;
                        }
                    }
                    
                    if (!dateParsed && !DateTime.TryParse(record.DateOfBirth, out dateOfBirth))
                    {
                        Console.WriteLine($"Skipping record {record.Name} - invalid date: {record.DateOfBirth}");
                        continue; // Пропускаем запись с неверной датой
                    }

                    // Проверяем и преобразуем булево значение
                    bool married = false;
                    if (!bool.TryParse(record.Married, out married))
                    {
                        // Пробуем альтернативные значения
                        if (record.Married.Equals("Yes", StringComparison.OrdinalIgnoreCase) || 
                            record.Married.Equals("Да", StringComparison.OrdinalIgnoreCase) ||
                            record.Married.Equals("1"))
                        {
                            married = true;
                        }
                        else if (record.Married.Equals("No", StringComparison.OrdinalIgnoreCase) || 
                                 record.Married.Equals("Нет", StringComparison.OrdinalIgnoreCase) ||
                                 record.Married.Equals("0"))
                        {
                            married = false;
                        }
                        else
                        {
                            continue; // Пропускаем запись с неверным булевым значением
                        }
                    }

                    // Проверяем и преобразуем зарплату с поддержкой различных форматов
                    decimal salary;
                    string salaryValue = record.Salary.Replace(",", "."); // Заменяем запятые на точки
                    
                    if (!decimal.TryParse(salaryValue, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out salary))
                    {
                        Console.WriteLine($"Skipping record {record.Name} - invalid salary: {record.Salary}");
                        continue; // Пропускаем запись с неверной зарплатой
                    }

                    var contact = new Contact
                    {
                        Name = record.Name,
                        DateOfBirth = dateOfBirth,
                        Married = married,
                        Phone = record.Phone,
                        Salary = salary
                    };
                    contacts.Add(contact);
                    conversionCount++;
                    Console.WriteLine($"Successfully converted contact: {contact.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting record {record.Name}: {ex.Message}");
                    // Пропускаем записи с ошибками преобразования
                    continue;
                }
            }
            
            Console.WriteLine($"CSV parsing completed. Total records: {records.Count}, Successfully converted: {conversionCount}, Final contacts: {contacts.Count}");
            return contacts;
        }
    }

    public sealed class ContactCsvMap : ClassMap<ContactCsvRecord>
    {
        public ContactCsvMap()
        {
            Map(m => m.Name).Name("Name", "Имя");
            Map(m => m.DateOfBirth).Name("DateOfBirth", "Date of birth", "Дата рождения");
            Map(m => m.Married).Name("Married", "Женат");
            Map(m => m.Phone).Name("Phone", "Телефон");
            Map(m => m.Salary).Name("Salary", "Зарплата");
        }
    }
}
