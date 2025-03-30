document.addEventListener("DOMContentLoaded", () => {
  const basePrice = parseFloat(
    document.getElementById("base-price").getAttribute("data-price")
  );
    
    const totalPriceElement = document.getElementById("total-price");
    const form = document.getElementById("order-form");

    form.addEventListener("change", updateTotalPrice);

    function updateTotalPrice() {
        let totalPrice = basePrice;
        const services = form.querySelectorAll('input[type="checkbox"]');

        services.forEach((service, index) => {
            if (service.checked) {
                const quantity =
                        parseInt(document.getElementById(`quantity${index + 1}`).value, 10) ||
                        0;
                totalPrice += parseFloat(service.value) * quantity;
            }
        });

        totalPriceElement.value = `$${totalPrice.toFixed(2)}`;
    }
});