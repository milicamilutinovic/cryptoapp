# CryptoApp

**CryptoApp** is a C# ASP.NET Core application designed for file encryption, decryption, and secure file exchange over a network. The application allows users to choose among multiple cryptographic algorithms, enables automatic encryption via file system monitoring, and supports TCP socket-based communication with other users for secure file transfer.

## Features

-  **Customizable Encryption**  
  Users can choose which algorithm to use for encrypting files. Supported algorithms:
  - **RC4** (Stream Cipher – Group 1)
  - **XTEA** (Block Cipher – Group 2)
  - **XTEA-CBC** (XTEA in CBC mode – Group 3)

-  **Cryptographic Hashing**  
  For file integrity verification during file exchange, the app uses:
  - **BLAKE** (Group 4 – Cryptographic Hash)

-  **File System Watcher (FSW)**  
  When enabled, the app monitors a selected folder (`Target Directory`) for new files. New files are automatically encrypted and stored in the encrypted folder (`EncryptedFilesDirectory`).

-  **Manual Encryption/Decryption**  
  When FSW is disabled, users can manually select files to encrypt or decrypt, choosing where to save the result.

-  **File Exchange via TCP Sockets**  
  The app supports sending and receiving encrypted files over the network. Files are transferred along with:
  - File name (`string`)
  - File size (`long`)
  - Hash length (`int`)
  - Hash value (`byte[]`)
  - Encrypted content (`byte[]`)

##  Folder Structure

- `wwwroot/uploads` – Temporary storage for uploaded files
- `wwwroot/received` – Files received via TCP
- `EncryptedFilesDirectory` – Configurable folder to store encrypted files
- `TargetDirectory` – Folder monitored by File System Watcher for new files

##  Application Settings

App settings can be configured in the **Settings page** of the application. The following options are available:

- Set `Target Directory` and `Encrypted Files Directory`
- Enable or disable:
  - FileWatcher service
  - File exchange (TCP)
- Select encryption algorithm: RC4, XTEA, or XTEA-CBC
- Choose hash algorithm: BLAKE

##  How File Exchange Works

1. **Sender:**
   - Selects a file
   - Encrypts it with selected algorithm
   - Computes hash (BLAKE)
   - Sends via TCP:
     - File name
     - File size
     - Hash length and hash value
     - Encrypted file content

2. **Receiver:**
   - Receives metadata and encrypted content
   - Verifies hash
   - Decrypts content if hash is valid
   - Saves the decrypted file

## Technologies Used

- ASP.NET Core Razor Pages
- C#
- FileSystemWatcher API
- TCP Client/Server (System.Net.Sockets)
- Custom Encryption Algorithms (RC4, XTEA, XTEA-CBC)
- Hashing with BLAKE

## Project Requirements (Summary)

- Implement one algorithm from each group (RC4, XTEA, CBC, BLAKE)
- Allow user to select algorithms
- Support key exchange (any method acceptable)
- Integrate FileSystemWatcher to detect added files
- Support TCP-based file exchange with other apps
- Verify file integrity using hash comparison

## Student Implementation

- **Stream Cipher (Group 1):** RC4  
- **Block Cipher (Group 2):** XTEA  
- **Block Cipher Mode (Group 3):** XTEA-CBC  
- **Cryptographic Hash (Group 4):** BLAKE  

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/milicamilutinovic/cryptoapp
   
2. Run the application:
   ```bash
    dotnet run

3. Open in browser:
   https://localhost:7159







