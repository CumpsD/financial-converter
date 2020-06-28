# Financial Converter

## Goal

> Convert various statement forms to another form so they can be imported.

## Support

### Reading

* CODA
* BNP Fortis

### Writing

* ING (csv for now, mt940 later)
* YNAB

## Usage

* Drop your CODA files in the Coda folder.
* Drop your BNP Paribas files in the Fortis folder.
* Have a look at the `appsettings.json` file to configure mappings.
* Run the program
* Collect your resulting files from your configured folders.

## Credits

The source of [supervos/coda-parser](https://github.com/supervos/coda-parser) has been included to parse the CODA statements and because there is no NuGet available.
