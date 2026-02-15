using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ContactManager.Core.Interfaces;
using ContactManager.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ContactManager.Web.Backend.Services
{
    /// <summary>
    /// Бэкэнд сервис для управления контактами
    /// Централизует бизнес-логику работы с контактами
    /// </summary>
    public class ContactBackendService
    {
        private readonly IContactService _contactService;
        private readonly ICsvService _csvService;
        private readonly ILogger<ContactBackendService> _logger;

        public ContactBackendService(
            IContactService contactService,
            ICsvService csvService,
            ILogger<ContactBackendService> logger)
        {
            _contactService = contactService;
            _csvService = csvService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все контакты из базы данных
        /// </summary>
        public async Task<IEnumerable<Contact>> GetAllContactsAsync()
        {
            try
            {
                _logger.LogInformation("Получение всех контактов из базы данных");
                var contacts = await _contactService.GetAllContactsAsync();
                _logger.LogInformation($"Получено {contacts.Count()} контактов");
                return contacts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении контактов из базы данных");
                throw;
            }
        }

        /// <summary>
        /// Получить контакт по ID
        /// </summary>
        public async Task<Contact> GetContactByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Получение контакта с ID: {id}");
                var contacts = await _contactService.GetAllContactsAsync();
                var contact = contacts.FirstOrDefault(c => c.Id == id);
                
                if (contact == null)
                {
                    throw new KeyNotFoundException($"Контакт с ID {id} не найден");
                }
                
                return contact;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении контакта с ID: {id}");
                throw;
            }
        }

        /// <summary>
        /// Обработка загрузки CSV файла
        /// </summary>
        public async Task<(int contactsAdded, string message)> ProcessCsvUploadAsync(IFormFile file)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Начало обработки CSV файла: {file.FileName}");

                if (file == null || file.Length == 0)
                {
                    return (0, "Файл не выбран или пустой");
                }

                // Проверка расширения файла
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".csv")
                {
                    return (0, "Пожалуйста, загрузите файл с расширением .csv");
                }

                // Проверка размера файла (макс 10MB)
                const int maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return (0, "Размер файла превышает 10MB");
                }

                // Чтение и парсинг CSV
                using var stream = file.OpenReadStream();
                var contacts = _csvService.ParseContacts(stream).ToList();

                if (!contacts.Any())
                {
                    return (0, "Файл не содержит контактов для импорта");
                }

                _logger.LogInformation($"Прочитано {contacts.Count} контактов из CSV файла");

                // Валидация контактов
                var validContacts = new List<Contact>();
                var validationErrors = new List<string>();

                for (int i = 0; i < contacts.Count; i++)
                {
                    var contact = contacts[i];
                    var validationResult = ValidateContact(contact);
                    
                    if (validationResult.IsValid)
                    {
                        validContacts.Add(contact);
                    }
                    else
                    {
                        validationErrors.Add($"Контакт {i + 1}: {string.Join(", ", validationResult.Errors)}");
                    }
                }

                _logger.LogInformation($"Валидных контактов: {validContacts.Count}, с ошибками: {validationErrors.Count}");

                // Импорт валидных контактов
                if (validContacts.Any())
                {
                    await _contactService.AddContactsAsync(validContacts);
                    _logger.LogInformation($"Добавлено {validContacts.Count} контактов в базу данных");
                }

                // Формирование результата
                var message = $"Импорт завершен. Добавлено контактов: {validContacts.Count} из {contacts.Count}.";
                
                if (validationErrors.Any())
                {
                    message += $" Пропущено из-за ошибок: {validationErrors.Count}.";
                    _logger.LogWarning($"Пропущено контактов из-за ошибок: {validationErrors.Count}");
                }

                stopwatch.Stop();
                _logger.LogInformation($"Обработка CSV файла завершена за {stopwatch.ElapsedMilliseconds}мс. Добавлено: {validContacts.Count}");

                return (validContacts.Count, message);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Критическая ошибка при обработке CSV файла");
                return (0, $"Ошибка обработки файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновить контакт
        /// </summary>
        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            try
            {
                _logger.LogInformation($"Обновление контакта с ID: {contact.Id}");
                
                var validationResult = ValidateContact(contact);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"Ошибка валидации при обновлении контакта {contact.Id}: {string.Join(", ", validationResult.Errors)}");
                    return false;
                }

                await _contactService.UpdateContactAsync(contact);
                _logger.LogInformation($"Контакт с ID {contact.Id} успешно обновлен");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении контакта с ID: {contact.Id}");
                return false;
            }
        }

        /// <summary>
        /// Удалить контакт
        /// </summary>
        public async Task<bool> DeleteContactAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Удаление контакта с ID: {id}");
                await _contactService.DeleteContactAsync(id);
                _logger.LogInformation($"Контакт с ID {id} успешно удален");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении контакта с ID: {id}");
                return false;
            }
        }

        /// <summary>
        /// Получить статистику по контактам
        /// </summary>
        public async Task<ContactStatistics> GetContactStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Получение статистики по контактам");
                var contacts = (await _contactService.GetAllContactsAsync()).ToList();

                var statistics = new ContactStatistics
                {
                    TotalContacts = contacts.Count,
                    MarriedContacts = contacts.Count(c => c.Married),
                    AverageSalary = contacts.Any() ? contacts.Average(c => (double)c.Salary) : 0,
                    MinSalary = contacts.Any() ? contacts.Min(c => c.Salary) : 0,
                    MaxSalary = contacts.Any() ? contacts.Max(c => c.Salary) : 0,
                    AverageAge = contacts.Any() ? contacts.Average(c => CalculateAge(c.DateOfBirth)) : 0
                };

                _logger.LogInformation($"Статистика получена: {statistics.TotalContacts} контактов");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики контактов");
                throw;
            }
        }

        /// <summary>
        /// Валидация контакта
        /// </summary>
        private ValidationResult ValidateContact(Contact contact)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(contact.Name))
            {
                errors.Add("Имя обязательно для заполнения");
            }
            else if (contact.Name.Length < 2 || contact.Name.Length > 100)
            {
                errors.Add("Имя должно содержать от 2 до 100 символов");
            }

            if (contact.DateOfBirth == default)
            {
                errors.Add("Дата рождения обязательна для заполнения");
            }
            else if (contact.DateOfBirth > DateTime.Now)
            {
                errors.Add("Дата рождения не может быть в будущем");
            }
            else if (CalculateAge(contact.DateOfBirth) > 150)
            {
                errors.Add("Возраст не может превышать 150 лет");
            }

            if (string.IsNullOrWhiteSpace(contact.Phone))
            {
                errors.Add("Телефон обязателен для заполнения");
            }
            else if (contact.Phone.Length < 5 || contact.Phone.Length > 20)
            {
                errors.Add("Телефон должен содержать от 5 до 20 символов");
            }

            if (contact.Salary < 0 || contact.Salary > 10000000)
            {
                errors.Add("Зарплата должна быть в диапазоне от 0 до 10,000,000");
            }

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors
            };
        }

        /// <summary>
        /// Рассчитать возраст
        /// </summary>
        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// Результат валидации
        /// </summary>
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }

        /// <summary>
        /// Статистика по контактам
        /// </summary>
        public class ContactStatistics
        {
            public int TotalContacts { get; set; }
            public int MarriedContacts { get; set; }
            public double AverageSalary { get; set; }
            public decimal MinSalary { get; set; }
            public decimal MaxSalary { get; set; }
            public double AverageAge { get; set; }
        }
    }
}