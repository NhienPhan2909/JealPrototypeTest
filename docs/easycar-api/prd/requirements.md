# Requirements

### Functional

**FR1:** The system shall store EasyCars credentials (Account Number/PublicID and Account Secret/SecretKey) securely for each dealership in the database with encryption at rest.

**FR2:** The CMS admin interface shall provide a "Dealership Settings" section where authorized administrators can view, create, update, and delete EasyCars credentials for their dealership.

**FR3:** The system shall implement JWT token-based authentication for all EasyCars API requests following the RequestToken endpoint specification (10-minute token validity).

**FR4:** The system shall integrate with the EasyCars Stock API to retrieve vehicle inventory data including all fields specified in the GetAdvertisementStocks endpoint response.

**FR5:** The system shall store all EasyCars Stock API fields in the database to maintain consistency with the source system, including: YardCode, YardName, StockNumber, Make, Model, Badge, RegoNum, VIN, Price, YearGroup, Odometer, Body, Colour, EngineCapacity, GearType, FuelType, AdvDescription, Series, AdvSpecialPrice, IsDemo, IsSpecial, IsPrestiged, StockType, IsUsed, RegoExpiry, VideoLink, DriveTrain, DoorNum, Cylinder, ShortDescription, EngineTypeDescription, EngineSize, StockStatus, GCM, GVM, Tare, SleepingCapacity, Toilet, Shower, AirConditioning, Fridge, Stereo, SeatCapacity, RegoState, AdditionalDescription, IsDriveAway, InteriorColor, BuiltDate, ComplianceDate, EngineNumber, GPS, Wheelsize, TowBallWeight, Warranty, Wheels, AxleConfiguration, Location, StandardFeatures, OptionalFeatures, RedbookCode, NVIC, IsMiles, GearCount, EnginePower, PowerkW, Powerhp, EngineMake, SerialNumber, Length, IsDepositTaken, ImageCount, and ImageURLs.

**FR6:** The system shall integrate with the EasyCars Lead API to synchronize customer leads including CreateLead, UpdateLead, and GetLeadDetail operations.

**FR7:** The system shall store all EasyCars Lead API fields in the database including: LeadNumber, StockNumber, VehicleMake, VehicleModel, VehiclePrice, VehicleType, VehicleYear, IsVehicleNew, VehicleInterest, Notes, VehicleSourcePageURL, FinanceStatus, Rating, ExternalID, CreatedDateTime, CustomerNo, CustomerName, CustomerEmail, CustomerAddress, CustomerCity, CustomerState, CustomerPostCode, CustomerFax, CustomerPhone, and CustomerMobile.

**FR8:** The system shall implement periodic automatic synchronization at configurable intervals (hourly, daily, or custom) to keep the local database synchronized with EasyCars data.

**FR9:** The CMS admin interface shall provide manual sync trigger buttons for both Stock and Lead data, allowing administrators to initiate immediate synchronization on demand.

**FR10:** The system shall display sync status and history in the admin interface, including last sync timestamp, sync duration, records processed, and any errors encountered.

**FR11:** The system shall implement intelligent sync logic to detect changes and only update modified records, reducing API calls and database operations.

**FR12:** The system shall map EasyCars Stock data to the existing vehicle inventory schema, creating or updating vehicle records as appropriate.

**FR13:** The system shall map EasyCars Lead data to the existing customer lead management system, creating new lead records and updating customer information as appropriate.

**FR14:** The system shall support test and production EasyCars API environments with separate base URLs (testmy.easycars.com.au for test, my.easycars.net.au for production).

**FR15:** The system shall handle EasyCars API response codes appropriately: Success (0), AuthenticationFail (1), Warning (5), Failed (7), and SystemError (9), with proper error logging and user notification.

**FR16:** The system shall support filtering stock synchronization by YardCode when configured, allowing dealerships to sync only specific yard inventories.

**FR17:** The system shall handle image synchronization from EasyCars, downloading and storing vehicle images referenced in the ImageURLs field.

**FR18:** The system shall track the source of each vehicle and lead record (e.g., "EasyCars", "Manual Entry") to support data lineage and conflict resolution.

**FR19:** The admin interface shall display clear indicators showing which vehicles and leads originated from EasyCars synchronization.

**FR20:** The system shall provide data reconciliation reports showing discrepancies between local data and EasyCars data, with options to resolve conflicts.

### Non Functional

**NFR1:** All EasyCars API credentials must be encrypted using AES-256 encryption at rest and transmitted over HTTPS/TLS 1.2 or higher.

**NFR2:** The synchronization process shall be designed as a background job that does not impact front-end user experience or response times.

**NFR3:** The system shall implement exponential backoff retry logic for failed API calls, with a maximum of 3 retry attempts before marking the sync as failed.

**NFR4:** API integration code shall be modular and loosely coupled to facilitate maintenance, testing, and potential future integration with other third-party systems.

**NFR5:** All API interactions shall be logged with appropriate detail levels (info, warning, error) to support troubleshooting and audit requirements.

**NFR6:** The synchronization process shall handle API rate limits gracefully, implementing throttling if necessary to stay within EasyCars API usage limits.

**NFR7:** The system shall maintain backward compatibility with existing vehicle and lead data structures, ensuring the integration does not break existing functionality.

**NFR8:** The integration shall support horizontal scaling to handle multiple dealerships synchronizing simultaneously without performance degradation.

**NFR9:** Database schema changes for EasyCars integration shall use migrations to ensure safe deployment and rollback capabilities.

**NFR10:** The system shall implement comprehensive error handling to prevent partial data corruption during synchronization failures, using database transactions where appropriate.

**NFR11:** The admin interface for credential management and sync operations shall be responsive and work on desktop, tablet, and mobile devices.

**NFR12:** API integration performance shall be monitored with metrics including sync duration, API response times, success/failure rates, and data throughput.
