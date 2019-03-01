# Data Scrapping for Prediction of Court Decision
The goal of the project is to make a simple tool is to process data extracted from __[court cases database](http://kad.arbitr.ru)__ and make it ready to apply machine learning techniques to predict court decision for the case: whether the case would be fully satisfied, partially satisfied or not satisfied at all.
## Scripts Description
Data scrapping scripts perform the following steps:
1. Load data from MS Access datasets
2. Extract data for following features:
    - Court Name
    - Instance
    - Case Category
    - Case Amount
    - List of Documents Attached to the case
    - Claimant features: presence at judicial sitting, number of claimants, is a person or a company
    - Respondent features: presence at judicial sitting, number of claimants, is a person or a company
    - Third party participation
3. Extract court decision and the name of the judge from the court decision pdf documents using regular expressions
4. Convert extracted data into categorical variables
5. Export data to csv
## Working with the Code
1. Clone the repository
2. Open and run the solution in Visual Studio 2012 or later versions
## External Libraries
__[iTextSharp](https://github.com/itext/itextsharp)__ library for parsing PDFs
