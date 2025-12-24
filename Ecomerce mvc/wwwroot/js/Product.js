document.addEventListener("DOMContentLoaded", function () {
    LoadDataTable();
});


var DataTable;
function LoadDataTable() {
    DataTable = $('#myTable').DataTable({
        "ajax": { url: '/Admin/Product/GetAll' },
        "columns": [
            { data: 'title', "width": "15%" },
            { data: 'author', "width": "15%" },
            { data: 'isbn', "width": "15%" },
            { data: 'price', "width": "15%" },
            { data: 'category.name', "width": "15%" },
            {
                data: 'id',
                'render': function (data) {
                    return `<div >
                       <a href="/Admin/Product/Upsert/${data}" class="btn btn-dark ps-4 pe-4 my-1" ><i class="bi bi-pencil-square"></i>Edit</a>
                       <a onClick = Delete('/Admin/Product/Delete/${data}') class="btn btn-danger"> <i class="bi bi-trash3-fill"></i>Delete</a>
                    </div>`
                }
            }

        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "delete it"
    }).then((result) => {
        if (result.isConfirmed) {
                $.ajax({
                    url: url,
                    type: 'DELETE',
                    success: function (data) {
                        DataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                })
        }
    });


}
