// Khởi tạo dữ liệu sản phẩm
const products = window.productData; // Tất cả sản phẩm
let filteredProducts = products.slice(); // Ban đầu, hiển thị tất cả sản phẩm

const productsPerPage = 12; // Số sản phẩm trên mỗi trang
let currentPage = 1;

// Lưu trữ các điều kiện lọc hiện tại
let selectedLoaiSanPham = null;
let selectedHangSanPham = null;

// Hàm hiển thị sản phẩm cho trang hiện tại
function displayProducts() {
    const start = (currentPage - 1) * productsPerPage;
    const end = start + productsPerPage;
    const paginatedProducts = filteredProducts.slice(start, end);

    const productList = document.getElementById('product-list');
    productList.innerHTML = ''; // Clear current products

    if (paginatedProducts.length === 0) {
        productList.innerHTML = '<p>Không tìm thấy sản phẩm nào.</p>';
        return;
    }

    paginatedProducts.forEach(product => {
        const productItem = `
            <div class="col-12 col-md-4 col-lg-3 mb-5 mb-md-0">
                <div class="product-item" data-id="${product.id}">
                    <a href="/Shop/DetailProduct?id=${product.id}" style="text-decoration: none;">
                        <img src="${product.image}" class="img-fluid product-thumbnail" />
                        <h3 class="product-title">${product.name}</h3>
                       <strong class="product-price">${product.price}</strong>
                    </a>
                    <a href="/Cart/AddToCart?idSanPham=${product.id}">
                        <span class="icon-cross">
                            <img src="/Content/Images/cross.svg" class="img-fluid">
                        </span>
                    </a>
                </div>
            </div>
        `; 
        productList.innerHTML += productItem;
    });
}


// Hàm tạo các điều khiển phân trang
function createPagination() {
    const totalPages = Math.ceil(filteredProducts.length / productsPerPage);
    const paginationControls = document.getElementById('pagination-controls');
    paginationControls.innerHTML = ''; // Xóa các liên kết phân trang hiện tại

    if (totalPages <= 1) {
        // Không cần phân trang nếu chỉ có một trang
        return;
    }

    for (let i = 1; i <= totalPages; i++) {
        const pageItem = `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
            </li>
        `;
        paginationControls.innerHTML += pageItem;
    }
}

// Hàm thay đổi trang hiện tại
function changePage(page) {
    currentPage = page;
    displayProducts();
    createPagination();
    // Cuộn lên đầu trang sau khi chuyển trang
    window.scrollTo(0, 0);
}

// Hàm lọc sản phẩm dựa trên các điều kiện đã chọn
function filterProducts() {
    filteredProducts = products.filter(product => {
        let isValid = true;

        if (selectedLoaiSanPham) {
            isValid = isValid && (product.loaiSanPham === selectedLoaiSanPham);
        }

        if (selectedHangSanPham) {
            isValid = isValid && (product.hangSanPham === selectedHangSanPham);
        }

        return isValid;
    });

    currentPage = 1; // Đặt lại trang hiện tại về 1
    displayProducts();
    createPagination();
}

// Thêm sự kiện cho các radio button khi trang đã tải xong
document.addEventListener('DOMContentLoaded', function () {
    // Thêm sự kiện cho các lựa chọn loại sản phẩm
    const loaiSanPhamInputs = document.querySelectorAll('input[name="loaiSanPham"]');
    loaiSanPhamInputs.forEach(input => {
        input.addEventListener('change', function () {
            // Toggle loại sản phẩm (bấm lại sẽ bỏ lọc)
            if (selectedLoaiSanPham === this.value) {
                selectedLoaiSanPham = null; // Bỏ lọc
                this.checked = false; // Bỏ chọn radio
            } else {
                selectedLoaiSanPham = this.value; // Chọn lọc mới
            }
            filterProducts();
        });
    });

    // Thêm sự kiện cho các lựa chọn hãng sản phẩm
    const hangSanPhamInputs = document.querySelectorAll('input[name="hangSanPham"]');
    hangSanPhamInputs.forEach(input => {
        input.addEventListener('change', function () {
            // Toggle hãng sản phẩm (bấm lại sẽ bỏ lọc)
            if (selectedHangSanPham === this.value) {
                selectedHangSanPham = null; // Bỏ lọc
                this.checked = false; // Bỏ chọn radio
            } else {
                selectedHangSanPham = this.value; // Chọn lọc mới
            }
            filterProducts();
        });
    });

    // Nếu bạn có nút "Clear Filters", thêm sự kiện cho nó
    const clearFiltersButton = document.getElementById('clear-filters');
    if (clearFiltersButton) {
        clearFiltersButton.addEventListener('click', function () {
            // Đặt lại các biến lọc
            selectedLoaiSanPham = null;
            selectedHangSanPham = null;

            // Bỏ chọn tất cả các radio button
            loaiSanPhamInputs.forEach(input => input.checked = false);
            hangSanPhamInputs.forEach(input => input.checked = false);

            // Lọc lại sản phẩm (hiển thị tất cả)
            filterProducts();
        });
    }

    // Khởi tạo hiển thị sản phẩm và phân trang
    displayProducts();
    createPagination();
});


