//Change total price
            document.addEventListener("DOMContentLoaded", () => {

                const totalPriceElement = document.getElementById("total-price");
                const form = document.getElementById("order-form");
// Chuyển giá trị của price thành số
    var number = parseFloat(totalPriceElement.value);

    // Định dạng số với dấu chấm
    var formattedNumber = number.toLocaleString('de-DE');

    // Gán giá trị đã định dạng lại cho thẻ h2
    totalPriceElement.value = formattedNumber;
    // format total price
                form.addEventListener("change", updateTotalPrice);
                
                function updateTotalPrice() {
                    let totalPrice = 0;

                    const services = form.querySelectorAll('input[type="checkbox"]');

                    services.forEach((service, index) => {
                        if (service.checked) {
                            console.log(service.alt);
                            const quantity = parseInt(document.getElementById(`quantity${index + 1}`).value, 10) || 0;
                            totalPrice += parseFloat(service.alt) * quantity;
                        }
                    });
                    // Chuyển giá trị của price thành số
        var number = parseFloat(totalPrice);

        // Định dạng số với dấu chấm
        var formattedNumber = number.toLocaleString('de-DE');

        // Gán giá trị đã định dạng lại cho thẻ h2
        totalPriceElement.value = formattedNumber;
//                    totalPriceElement.value = `${totalPrice}`;
                }

                // Adding event listeners for increment and decrement buttons
                document.querySelectorAll('.increment').forEach(button => {
                    button.addEventListener('click', function () {

                        updateTotalPrice();
                    });

                });


                document.querySelectorAll('.decrement').forEach(button => {
                    button.addEventListener('click', function () {
                        updateTotalPrice();
                    });
                });
            });


