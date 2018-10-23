var InnAddress = {
    data: [],
    init: function () {
        $(document)
            .on('change', '.jsChangeInnAddress', InnAddress.changeEvent);

        try {
            var data = $("#InnAddressData").val();
            if (data) {
                InnAddress.data = JSON.parse(data);
            }
        } catch (e) {
            console.log("Couldn't parse INN addresses", e);
        }
        
        try {
            var preselectedAddress = $("#InnAddressData").data("preselect");
            if (preselectedAddress && preselectedAddress instanceof Object) {
                InnAddress.changeInnAddress(preselectedAddress);
            }
        } catch (e) {
            console.log("Couldn't set INN preselected address", e);
        }
    },
    changeEvent: function () {
        var selectedValue = $(this).val();
        var address = InnAddress.getAddress(selectedValue);
        InnAddress.changeInnAddress(address);
    },
    changeInnAddress: function (address) {
        if (address) {
            InnAddress.fillFields("#BillingAddress", address);
            InnAddress.fillFields("#Shipments_0__Address", address);
        } else {
            InnAddress.clearFields("#BillingAddress");
            InnAddress.clearFields("#Shipments_0__Address");
        }
    },
    fillFields: function (prefix, address) {
        if ($(prefix + "_FirstName").length) {
            var splitName = address.Contact.Name.split(" ");
            $(prefix + "_FirstName").val(splitName.length > 0 ? splitName[0] : "");
            $(prefix + "_LastName").val(splitName.length > 1 ? splitName.slice(1).join(" ") : "");
            $(prefix + "_Email").val(address.Contact.Email);
            $(prefix + "_Line1").val(address.AddressLine1);
            $(prefix + "_PostalCode").val(address.Postalcode);
            $(prefix + "_City").val(address.Postalcity);
            if (address.CountryCode === "no") {
                var countrySelect = $(prefix + "_CountryCode");
                countrySelect.val("NOR");
                AddressBook.setRegion.call(countrySelect[0]);
            }
        }
    },
    clearFields: function (prefix) {
        if ($(prefix + "_FirstName").length) {
            $(prefix + "_FirstName").val("");
            $(prefix + "_LastName").val("");
            $(prefix + "_Email").val("");
            $(prefix + "_Line1").val("");
            $(prefix + "_PostalCode").val("");
            $(prefix + "_City").val("");
            $(prefix + "_CountryCode").val("USA");
        }
    },
    getAddress: function (addressId) {
        var length = InnAddress.data.length;
        for (var i = 0; i < length; i++) {
            if (InnAddress.data[i].Tags === addressId) {
                return InnAddress.data[i]; 
            }
        }
    }
}