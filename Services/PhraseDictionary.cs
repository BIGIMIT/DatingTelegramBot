using DatingTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatingTelegramBot.Services;


public static class PhraseDictionary
{
    private static readonly Dictionary<string, Dictionary<Phrases, string>> _phrases = new Dictionary<string, Dictionary<Phrases, string>>
    {
        ["English"] = new Dictionary<Phrases, string>
        {
            [Phrases.Accept_the_agreement] = "Accept the agreement!",
            [Phrases.Set_up_your_Username_and_try_again] = "Set up your Username and try again.",
            [Phrases.I_agree] = "I agree",
            [Phrases.Continue] = "Continue",
            [Phrases.Create_an_account] = "Create an account",
            [Phrases.Please_enter_your_name] = "Please enter your name",
            [Phrases.Now_enter_your_age] = "Now enter your age",
            [Phrases.You_must_be_over_18_years_old] = "You must be over 18 years old",
            [Phrases.Woman] = "Woman",
            [Phrases.Man] = "Man",
            [Phrases.Both] = "Both",
            [Phrases.View_my_profile] = "View my profile",
            [Phrases.Complete_registration] = "Complete registration",
            [Phrases.Write_semething_bout_you] = "Write something about you",
            [Phrases.Choose_preferred_gender] = "Choose preferred gender",
            [Phrases.Use_the_keyboard_below] = "Use the keyboard below",
            [Phrases.Name] = "Name",
            [Phrases.Age] = "Age",
            [Phrases.Gender] = "Gender",
            [Phrases.Messege] = "Message",
            [Phrases.Description] = "Description",
            [Phrases.Choose_your_gender] = "Choose your gender",
            [Phrases.Resume_searching] = "Resume searching",
            [Phrases.Next] = "Next",
            [Phrases.Matches] = "Matches",
            [Phrases.Back_to_searching] = "Back to searching",
            [Phrases.Stop_searching] = "Stop searching",
            [Phrases.Select_an_item] = "Select an item",
            [Phrases.Try_again] = "Try again",
            [Phrases.No_more_matches_available] = "No more matches available",
            [Phrases.Yes_Send] = "Yes, Send",
            [Phrases.Rewrite] = "Rewrite",
            [Phrases.Are_you_sure_you_want_to_send_a_message] = "Are you sure you want to send a message?",
            [Phrases.Message_saved_successfully] = "Message saved successfully",
            [Phrases.Message_not_found_please_try_again] = "Message not found, please try again",
            [Phrases.Write_a_new_message] = "Write a new message",
            [Phrases.Change_profile] = "Change profile",
            [Phrases.ChangeAccountHandler] = "Change Account Handler",
            [Phrases.Could_not_find_file] = "Could not find file",
            [Phrases.Your_account_has_become_invisible_to_other_users] = "Your account has become invisible to other users",
            [Phrases.Write_some_message] = "Write some message",
            [Phrases.No_more_profiles_available] = "No more profiles available",
            [Phrases.Something_went_wrong_try_again] = "Something went wrong, try again",
            [Phrases.Thats_almost_all_it_remains_only_to_add_a_photo] = "That's almost all, it remains only to add a photo",
            [Phrases.Choose_next_step] = "Choose next step",

        },
        ["Русский"] = new Dictionary<Phrases, string>
        {
            [Phrases.Accept_the_agreement] = "Принять соглашение",
            [Phrases.Set_up_your_Username_and_try_again] = "Установите свой Username и попробуйте снова.",
            [Phrases.I_agree] = "Я соглашаюсь",
            [Phrases.Continue] = "Продолжить",
            [Phrases.Create_an_account] = "Создать аккаунт",
            [Phrases.Please_enter_your_name] = "Пожалуйста, введите ваше имя",
            [Phrases.Now_enter_your_age] = "Теперь введите свой возраст",
            [Phrases.You_must_be_over_18_years_old] = "Вам должно быть более 18 лет",
            [Phrases.Woman] = "Женщина",
            [Phrases.Man] = "Мужчина",
            [Phrases.Both] = "Оба",
            [Phrases.View_my_profile] = "Посмотреть мой профиль",
            [Phrases.Complete_registration] = "Завершить регистрацию",
            [Phrases.Write_semething_bout_you] = "Напишите что-нибудь о себе",
            [Phrases.Choose_preferred_gender] = "Выберите предпочтительный пол",
            [Phrases.Use_the_keyboard_below] = "Используйте клавиатуру ниже",
            [Phrases.Name] = "Имя",
            [Phrases.Age] = "Возраст",
            [Phrases.Gender] = "Пол",
            [Phrases.Messege] = "Сообщение",
            [Phrases.Description] = "Описание",
            [Phrases.Choose_your_gender] = "Выберите свой пол",
            [Phrases.Resume_searching] = "Продолжить поиск",
            [Phrases.Next] = "Далее",
            [Phrases.Matches] = "Совпадения",
            [Phrases.Back_to_searching] = "Вернуться к поиску",
            [Phrases.Stop_searching] = "Прекратить поиск",
            [Phrases.Select_an_item] = "Выберите пункт",
            [Phrases.Try_again] = "Попробуйте снова",
            [Phrases.No_more_matches_available] = "Больше нет совпадений",
            [Phrases.Yes_Send] = "Да, отправить",
            [Phrases.Rewrite] = "Переписать",
            [Phrases.Are_you_sure_you_want_to_send_a_message] = "Вы уверены, что хотите отправить сообщение?",
            [Phrases.Message_saved_successfully] = "Сообщение успешно сохранено",
            [Phrases.Message_not_found_please_try_again] = "Сообщение не найдено, пожалуйста, попробуйте снова",
            [Phrases.Write_a_new_message] = "Написать новое сообщение",
            [Phrases.Change_profile] = "Изменить профиль",
            [Phrases.ChangeAccountHandler] = "Изменить обработчик аккаунта",
            [Phrases.Could_not_find_file] = "Не удалось найти файл",
            [Phrases.Your_account_has_become_invisible_to_other_users] = "Ваш аккаунт стал невидимым для других пользователей",
            [Phrases.Write_some_message] = "Напишите какое-то сообщение",
            [Phrases.No_more_profiles_available] = "Больше нет профилей для просмотра",
            [Phrases.Something_went_wrong_try_again] = "Что-то пошло не так, попробуйте снова",
            [Phrases.Thats_almost_all_it_remains_only_to_add_a_photo] = "Осталось только добавить фото",
            [Phrases.Choose_next_step] = "Выберите следующий шаг",
        },
        ["Українська"] = new Dictionary<Phrases, string>
        {
            [Phrases.Accept_the_agreement] = "Прийняти Угоду",
            [Phrases.Set_up_your_Username_and_try_again] = "Встановіть свій Username і спробуйте ще раз.",
            [Phrases.I_agree] = "Я погоджуюсь",
            [Phrases.Continue] = "Продовжити",
            [Phrases.Create_an_account] = "Створити обліковий запис",
            [Phrases.Please_enter_your_name] = "Будь ласка, введіть своє ім'я",
            [Phrases.Now_enter_your_age] = "Тепер введіть свій вік",
            [Phrases.You_must_be_over_18_years_old] = "Вам повинно бути більше 18 років",
            [Phrases.Woman] = "Жінка",
            [Phrases.Man] = "Чоловік",
            [Phrases.Both] = "Обидва",
            [Phrases.View_my_profile] = "Переглянути мій профіль",
            [Phrases.Complete_registration] = "Завершити реєстрацію",
            [Phrases.Write_semething_bout_you] = "Напишіть щось про себе",
            [Phrases.Choose_preferred_gender] = "Виберіть бажану стать",
            [Phrases.Use_the_keyboard_below] = "Використовуйте клавіатуру нижче",
            [Phrases.Name] = "Ім'я",
            [Phrases.Age] = "Вік",
            [Phrases.Gender] = "Стать",
            [Phrases.Messege] = "Повідомлення",
            [Phrases.Description] = "Опис",
            [Phrases.Choose_your_gender] = "Виберіть свою стать",
            [Phrases.Resume_searching] = "Продовжити пошук",
            [Phrases.Next] = "Далі",
            [Phrases.Matches] = "Збіги",
            [Phrases.Back_to_searching] = "Назад до пошуку",
            [Phrases.Stop_searching] = "Зупинити пошук",
            [Phrases.Select_an_item] = "Виберіть пункт",
            [Phrases.Try_again] = "Спробуйте ще раз",
            [Phrases.No_more_matches_available] = "Більше профілів немає",
            [Phrases.Yes_Send] = "Так, відправити",
            [Phrases.Rewrite] = "Перепишіть",
            [Phrases.Are_you_sure_you_want_to_send_a_message] = "Ви впевнені, що хочете відправити повідомлення?",
            [Phrases.Message_saved_successfully] = "Повідомлення успішно збережено",
            [Phrases.Message_not_found_please_try_again] = "Повідомлення не знайдено, спробуйте ще раз",
            [Phrases.Write_a_new_message] = "Напишіть нове повідомлення",
            [Phrases.Change_profile] = "Змінити профіль",
            [Phrases.ChangeAccountHandler] = "Змінити обробник облікового запису",
            [Phrases.Could_not_find_file] = "Не вдалося знайти файл",
            [Phrases.Your_account_has_become_invisible_to_other_users] = "Ваш обліковий запис став невидимим для інших користувачів",
            [Phrases.Write_some_message] = "Напишіть деяке повідомлення",
            [Phrases.No_more_profiles_available] = "Більше профілів немає",
            [Phrases.Something_went_wrong_try_again] = "Щось пішло не так, спробуйте ще раз",
            [Phrases.Thats_almost_all_it_remains_only_to_add_a_photo] = "Залишилося додати тільки фото",
            [Phrases.Choose_next_step] = "Виберіть наступний крок",
        }

    };

    public static string GetPhrase(string? language, Phrases phrase)
    {
        if (language == null)
        {
            return "Please starting from /start";
        }
        string languageKey = language.ToString();
        if (_phrases.TryGetValue(languageKey, out var languagePhrases) && languagePhrases.TryGetValue(phrase, out string? result))
        {
            return result;
        }
        else
        {
            return "Phrase not found.";
        }
    }
}