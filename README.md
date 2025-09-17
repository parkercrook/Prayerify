# 🙏 Prayerify

A beautiful and intuitive prayer management app built with .NET MAUI, designed to help you organize, track, and engage with your prayers in a meaningful way.

## ✨ Features

### 📝 Prayer Management
- **Create & Edit Prayers**: Add detailed prayers with subjects and body text
- **Organize by Categories**: Group prayers into custom categories for better organization
- **Mark as Answered**: Track when prayers are answered to celebrate God's faithfulness
- **Soft Delete**: Safely remove prayers without permanent data loss

### 🎯 Prayer Sessions
- **Guided Prayer Sessions**: Set up focused prayer time with a specific number of prayers
- **Random Selection**: Prayers are randomly selected for each session to ensure variety
- **Session Flow**: Navigate through prayers one by one with options to mark as done or answered
- **Flexible Count**: Choose how many prayers to include in each session

### 📊 Prayer Tracking
- **Active Prayers**: View all your current, unanswered prayers
- **Answered Prayers**: Celebrate and review prayers that have been answered
- **Category Management**: Create, edit, and organize prayer categories
- **Date Tracking**: Automatic timestamps for when prayers are created

### 🎨 User Experience
- **Cross-Platform**: Runs on Android, iOS, Windows, and macOS
- **Modern UI**: Clean, intuitive interface with tab-based navigation
- **Responsive Design**: Optimized for different screen sizes and orientations
- **Offline First**: All data stored locally with SQLite database

## 🏗️ Architecture

### Technology Stack
- **.NET MAUI**: Cross-platform mobile and desktop development
- **MVVM Pattern**: Clean separation of concerns with CommunityToolkit.Mvvm
- **SQLite**: Local database for prayer storage
- **Dependency Injection**: Built-in service registration and management

### Project Structure
```
Prayerify/
├── Models/           # Data models (Prayer, Category)
├── ViewModels/       # MVVM ViewModels for each page
├── Pages/           # XAML UI pages
├── Data/            # Database layer and interfaces
├── Services/        # Application services (Dialog, etc.)
├── Resources/       # Images, fonts, and styling
└── Platforms/       # Platform-specific implementations
```

### Key Components
- **PrayerDatabase**: SQLite-based data access layer
- **BaseViewModel**: Common functionality for all ViewModels
- **DialogService**: Cross-platform dialog management
- **AppShell**: Tab-based navigation structure

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with MAUI workload
- Platform-specific development tools:
  - **Android**: Android SDK and emulator
  - **iOS**: Xcode and iOS Simulator (macOS only)
  - **Windows**: Windows 10/11 SDK

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/prayerify.git
   cd prayerify
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the app**
   ```bash
   # For Android
   dotnet build -t:Run -f net8.0-android
   
   # For Windows
   dotnet build -t:Run -f net8.0-windows10.0.19041.0
   
   # For iOS (macOS only)
   dotnet build -t:Run -f net8.0-ios
   ```

### Development Setup

1. **Open in Visual Studio**
   - Open `Prayerify.sln` in Visual Studio 2022
   - Ensure MAUI workload is installed
   - Select your target platform and run

2. **Database Location**
   - The SQLite database is automatically created at: `{AppDataDirectory}/prayerify.db3`
   - No manual database setup required

## 📱 Usage

### Adding Prayers
1. Navigate to the **Prayers** tab
2. Tap the **+** button to add a new prayer
3. Enter a subject and detailed prayer text
4. Optionally assign to a category
5. Save your prayer

### Organizing with Categories
1. Go to the **Categories** tab
2. Add new categories to organize your prayers
3. Assign prayers to categories when creating or editing them

### Prayer Sessions
1. Visit the **Session** tab
2. Choose how many prayers to include (1-10)
3. Tap **Start Session** to begin
4. Navigate through prayers with **Next** or **Mark Answered**
5. Complete the session when finished

### Tracking Answered Prayers
1. Use the **Answered** tab to view all answered prayers
2. Celebrate God's faithfulness and answered prayers
3. Review past prayers for encouragement

## 🧪 Testing

The project includes comprehensive unit tests using xUnit, Moq, and FluentAssertions.

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "CategoryTests"
```

### Test Coverage
- **Models**: Property validation and default values
- **Database**: CRUD operations and data integrity
- **ViewModels**: Business logic and state management
- **Services**: Cross-platform service implementations
- **Integration**: End-to-end workflow testing

## 🔧 Configuration

### Database
- **Type**: SQLite
- **Location**: `{AppDataDirectory}/prayerify.db3`
- **Auto-initialization**: Database and tables created automatically

### Dependencies
- **CommunityToolkit.Mvvm**: MVVM framework
- **sqlite-net-pcl**: SQLite database access
- **Humanizer.Core**: Text formatting utilities

## 📦 Building for Release

### Android
```bash
dotnet publish -f net8.0-android -c Release
```

### Windows
```bash
dotnet publish -f net8.0-windows10.0.19041.0 -c Release
```

### iOS
```bash
dotnet publish -f net8.0-ios -c Release
```

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Write unit tests for new features
- Ensure cross-platform compatibility

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](Prayerify/LICENSE.txt) file for details.

## 📞 Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/parkercrook/prayerify/issues) page
2. Create a new issue with detailed information

---

**Prayerify** - Making prayer a more organized and meaningful part of your daily life. 🙏✨
