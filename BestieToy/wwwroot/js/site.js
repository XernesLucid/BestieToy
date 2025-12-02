// BestieToy - Main JavaScript File

$(document).ready(function () {
    console.log('BestieToy loaded');

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert').alert('close');
    }, 5000);

    // Cart badge update
    updateCartBadge();

    // Search functionality
    $('#searchForm').on('submit', function (e) {
        var searchTerm = $('#searchInput').val().trim();
        if (searchTerm === '') {
            e.preventDefault();
            $('#searchInput').focus();
        }
    });

    // Add to cart AJAX
    $('.add-to-cart').on('click', function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var quantity = $(this).data('quantity') || 1;

        $.ajax({
            url: '/Cart/Add',
            type: 'POST',
            data: {
                productId: productId,
                quantity: quantity,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    showToast('Đã thêm vào giỏ hàng!', 'success');
                    updateCartBadge(response.itemCount);
                } else {
                    if (response.redirect) {
                        window.location.href = response.redirect;
                    } else {
                        showToast(response.message || 'Có lỗi xảy ra', 'error');
                    }
                }
            },
            error: function () {
                showToast('Lỗi kết nối máy chủ', 'error');
            }
        });
    });

    // Update cart quantity
    $('.update-quantity').on('change', function () {
        var cartItemId = $(this).data('cart-item-id');
        var quantity = $(this).val();

        if (quantity < 1) {
            quantity = 1;
            $(this).val(1);
        }

        updateCartItem(cartItemId, quantity);
    });

    // Remove from cart
    $('.remove-from-cart').on('click', function (e) {
        e.preventDefault();
        var cartItemId = $(this).data('cart-item-id');

        if (confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            removeCartItem(cartItemId);
        }
    });

    // Product image zoom
    $('.product-image').on('mouseenter', function () {
        $(this).addClass('zoomed');
    }).on('mouseleave', function () {
        $(this).removeClass('zoomed');
    });

    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function (e) {
        if ($(this).attr('href') !== '#') {
            e.preventDefault();
            var target = $(this.hash);
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 80
                }, 800);
            }
        }
    });
});

// Function to update cart badge
function updateCartBadge(count) {
    if (count !== undefined) {
        $('.cart-badge').text(count);
    } else {
        // Fetch current cart count via AJAX
        $.get('/Cart/GetItemCount', function (data) {
            $('.cart-badge').text(data.itemCount || 0);
        });
    }
}

// Function to update cart item
function updateCartItem(cartItemId, quantity) {
    $.ajax({
        url: '/Cart/Update',
        type: 'POST',
        data: {
            cartItemId: cartItemId,
            quantity: quantity,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                // Update totals in cart page
                $('#subtotal').text(response.subtotal);
                $('#shippingFee').text(response.shippingFee);
                $('#tax').text(response.tax);
                $('#total').text(response.total);
                updateCartBadge(response.itemCount);
                showToast('Đã cập nhật số lượng', 'success');
            } else {
                showToast(response.message || 'Có lỗi xảy ra', 'error');
            }
        }
    });
}

// Function to remove cart item
function removeCartItem(cartItemId) {
    $.ajax({
        url: '/Cart/Remove',
        type: 'POST',
        data: {
            cartItemId: cartItemId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                // Remove item row from table
                $('#cart-item-' + cartItemId).fadeOut(300, function () {
                    $(this).remove();
                    if ($('.cart-item').length === 0) {
                        $('#cart-table').html('<tr><td colspan="5" class="text-center">Giỏ hàng trống</td></tr>');
                    }
                });

                // Update totals
                $('#subtotal').text(response.subtotal);
                $('#shippingFee').text(response.shippingFee);
                $('#tax').text(response.tax);
                $('#total').text(response.total);
                updateCartBadge(response.itemCount);
                showToast('Đã xóa sản phẩm', 'success');
            } else {
                showToast(response.message || 'Có lỗi xảy ra', 'error');
            }
        }
    });
}

// Toast notification function
function showToast(message, type) {
    var toastClass = 'bg-primary';
    if (type === 'error') toastClass = 'bg-danger';
    if (type === 'success') toastClass = 'bg-success';
    if (type === 'warning') toastClass = 'bg-warning';

    var toastHtml = `
        <div class="toast align-items-center text-white ${toastClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    var $toast = $(toastHtml);
    $('#toastContainer').append($toast);
    var toast = new bootstrap.Toast($toast[0]);
    toast.show();

    // Remove toast after hide
    $toast.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// Price formatter
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

// Initialize when needed
$(function () {
    // Format all price elements
    $('.price').each(function () {
        var price = parseFloat($(this).text().replace(/[^0-9.-]+/g, ""));
        if (!isNaN(price)) {
            $(this).text(formatPrice(price));
        }
    });
});