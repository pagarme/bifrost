<img src="https://cdn.rawgit.com/pagarme/brand/9ec30d3d4a6dd8b799bca1c25f60fb123ad66d5b/logo-circle.svg" width="127px" height="127px" align="left"/>

# Bifrost

Project to create communication between mpos and browsers on Windows / Linux Ubuntu.

## Usage

Currently the bridge exposes mPOS devices on an websocket endpoint.

In the future, SOAP/WCF and REST implementations are expected.

## Documentation

You can find more details about this project [here](docs/).

## License

See [here](LICENSE.md).

## Components

### PagarMe.Bifrost

Actual bridge implementation.

### PagarMe.Bifrost.Certificates

Create necessary certificates to communicate as HTTPS.

### PagarMe.Bifrost.Example

Project to run Bifrost locally to make tests.

### PagarMe.Bifrost.Linux / PagarMe.Bifrost.Linux.Deps

Build to create linux zip.

### PagarMe.Bifrost.Windows / PagarMe.Bifrost.Windows.Deps

Build to create windows msi installer.

### PagarMe.Bifrost.Service

Bridge server as a Windows service.

### PagarMe.Bifrost.Setup.Helper

Copies right files after Linux / Windows "installers" builds.

### PagarMe.Generic

Contains generic methods to help other projects
