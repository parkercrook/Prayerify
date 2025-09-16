# Prayerify Unit Tests

This project contains comprehensive unit tests for the Prayerify application using xUnit, Moq, and FluentAssertions.

## Test Structure

### Models Tests
- **PrayerTests.cs** - Tests for the Prayer model including property validation and default values
- **CategoryTests.cs** - Tests for the Category model including property validation and default values

### Data Layer Tests
- **PrayerDatabaseTests.cs** - Integration tests for the PrayerDatabase class using in-memory SQLite database

### ViewModels Tests
- **BaseViewModelTests.cs** - Tests for the base ViewModel functionality
- **PrayersViewModelTests.cs** - Tests for the PrayersViewModel including CRUD operations and business logic
- **EditPrayerViewModelTests.cs** - Tests for the EditPrayerViewModel including validation and save operations
- **CategoriesViewModelTests.cs** - Tests for the CategoriesViewModel including category management

### Services Tests
- **DialogServiceTests.cs** - Tests for the DialogService interface and implementation

### Integration Tests
- **PrayerWorkflowTests.cs** - End-to-end workflow tests that test complete user scenarios

## Running Tests

### From Command Line
```bash
dotnet test
```

### From Visual Studio
1. Open the solution in Visual Studio
2. Go to Test > Test Explorer
3. Click "Run All Tests"

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

- **Unit Tests**: Test individual components in isolation using mocks
- **Integration Tests**: Test component interactions using real database
- **Workflow Tests**: Test complete user scenarios end-to-end

## Dependencies

- **xUnit**: Testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Fluent assertion library for readable test assertions
- **SQLite**: In-memory database for integration tests

## Test Data

Tests use temporary SQLite databases that are created and destroyed for each test to ensure isolation and prevent test interference.

## Coverage

The tests aim to achieve high code coverage across:
- All public methods and properties
- Edge cases and error conditions
- Business logic validation
- Data persistence operations
- User interface interactions (via ViewModels)

