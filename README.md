# YTU Erasmus Bot

A Telegram bot for Yildiz Technical University (YTU) that sends announcements from the Mathematics and Erasmus departments to subscribers.

## Description

This bot fetches announcements from the YTU Mathematics and Erasmus departments and sends them to subscribed users on Telegram. The bot can handle multiple commands to start and stop receiving notifications for different types of announcements.

## Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/jamwitk/ytu-erasmus-bot.git
   cd ytu-erasmus-bot
   ```

2. Install the necessary dependencies.

3. Set up your Telegram bot by obtaining a bot token from BotFather and replacing the placeholder in the code.

4. Run the bot:
   ```sh
   dotnet run
   ```

## Usage

### Commands

- **Start receiving Mathematics announcements:**
  ```
  /matduyurubaslat
  ```

- **Stop receiving Mathematics announcements:**
  ```
  /matduyurukapat
  ```

- **Start receiving Erasmus announcements:**
  ```
  /starterasmus
  ```

- **Stop receiving Erasmus announcements:**
  ```
  /stoperasmus
  ```

### Features

- Automatically fetches and sends new announcements from the YTU website.
- Subscribes users to announcements based on their preferences.

## Contributing

1. Fork the repository.
2. Create your feature branch (`git checkout -b feature/my-new-feature`).
3. Commit your changes (`git commit -am 'Add some feature'`).
4. Push to the branch (`git push origin feature/my-new-feature`).
5. Create a new Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For any inquiries or support, please contact me.
