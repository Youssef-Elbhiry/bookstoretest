document.addEventListener("DOMContentLoaded", function () {
    LoadDataTable();
});

var datatable;
function LoadDataTable() {
   datatable = $('#myTable').DataTable({
        "ajax": {url : '/Admin/Company/GetAll'},
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'city', "width": "15%" },
            { data: 'state', "width": "15%" },
            { data: 'streetAddress', "width": "15%" },
            {
                data: 'id',
                'render': function (data) {
                    return `<div >
                       <a href="/Admin/Company/Upsert/${data}" class="btn btn-dark ps-4 pe-4 my-1" ><i class="bi bi-pencil-square"></i>Edit</a>
                       <a onClick = Delete('/Admin/Company/Delete/${data}') class="btn btn-danger"> <i class="bi bi-trash3-fill"></i>Delete</a>
                    </div>`
                }   
                , "width": "25%" 
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
                type: 'Delete',
                success: function (data) {
                    datatable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });


}

