document.addEventListener('DOMContentLoaded', function () {

    LoadDataTable();
});

var datatable;
function LoadDataTable() {

  datatable = $('#myTable').DataTable({
        ajax: '/Admin/User/GetAllUsers',
        columns: [
            { data: 'name', width: '15%' },
            { data: 'email', width: '15%' },
            { data: 'phoneNumber', width: '15%' },
            { data: 'company.name', width: '15%' },
            { data: 'role', width: '15%' },
            {
                data: { id: 'id', lockoutEnd:'lockoutEnd'},
                render: function (data) {

                    var now = new Date().getTime();
                    var lockoutEnd = new Date(data.lockoutEnd).getTime();
                    if (lockoutEnd > now) {
                        // User is locked
                        return `<div>
                        <a onclick="LockUnLock('${data.id}')" class="btn btn-danger mx-2"><i class="bi bi-lock-fill"></i></a>
                        </div>`;
                    }
                    else {
                        // User is not locked
                        return `<div>
                        <a onclick="LockUnLock('${data.id}')" class="btn btn-success mx-2 "><i class="bi bi-unlock-fill"></i></a>
                        </div>`;
                    } }
                   
                ,
                width: '10%'
            }
        ]

    });
}

function LockUnLock(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/User/LockUnLock',
        data: JSON.stringify(id),
        contentType: 'application/json',
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                datatable.ajax.reload();
            }
            
        }
    })
}