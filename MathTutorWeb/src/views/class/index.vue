<template>
    <div class="mixin-components-container">
        <el-row>
            <el-card class="box-card">
                <div style="margin-bottom:50px;">
                    <el-col :span="4" class="text-center">
                        <el-button type="primary" @click="dialogVisible = true">
                            添加班级
                        </el-button>
                        <el-dialog v-el-drag-dialog title="添加班级" :visible.sync="dialogVisible" width="30%"
                            @close="resetForm">
                            <el-form ref="classForm" :model="form" :rules="rules" label-width="100px">
                                <el-form-item label="班级名称" prop="className">
                                    <el-input v-model="form.className" placeholder="如：微积分_21计科3班" clearable>
                                    </el-input>
                                </el-form-item>
                            </el-form>
                            <span slot="footer" class="dialog-footer">
                                <el-button type="primary" @click="submitForm">确 定</el-button>
                            </span>
                        </el-dialog>
                    </el-col>
                </div>
            </el-card>
        </el-row>
        <el-row :gutter="20" style="margin-top:50px;">
            <el-col v-for="(item, index) in classList" :key="index" :span="6" style="margin-bottom:20px;">
                <el-card class="box-card" @click.native="handleClickClass(item.classId)">
                    <div class="component-item" style="font-size: 20px;padding-top: 36px;">
                        <mallki class-name="mallki-text" :text="item.className" />
                    </div>
                    <div>
                        学生数量：{{ item.StudentCount }}
                    </div>
                </el-card>
            </el-col>
        </el-row>
    </div>
</template>
  
<script>
import Mallki from '@/components/TextHoverEffect/Mallki'
import elDragDialog from '@/directive/el-drag-dialog'
import { createClass, getMyClasses } from '@/api/class'


export default {
    name: 'ClassList',
    directives: { elDragDialog },
    components: {
        Mallki,
    },
    data() {
        return {
            dialogVisible: false,
            classList: [], // 班级列表数据
            form: {
                className: '微积分_21计科3班'
            },
            rules: {
                className: [
                    { required: true, message: '班级名称不能为空', trigger: 'blur' }
                ]
            }
        }
    },
    created() {
        this.loadClasses()
    },
    methods: {
        // 加载班级列表
        async loadClasses() {
            try {
                const res = await getMyClasses()
                if (res.code === 20000) {
                    this.classList = res.data
                }
            } catch (error) {
                console.error('加载班级列表失败:', error)
            }
        },
        // 提交表单
        submitForm() {
            this.$refs.classForm.validate(async valid => {
                if (valid) {
                    try {
                        const res = await createClass(this.form)
                        if (res.code === 20000) {
                            this.$message.success('创建成功')
                            this.dialogVisible = false
                            this.loadClasses() // 刷新列表
                        }
                    } catch (error) {
                        this.$message.error('创建失败：' + (error.message || '请求异常'))
                    }
                }
            })
        },
        // 重置表单
        resetForm() {
            this.$refs.classForm.resetFields()
        },

        // 保留原有的拖拽处理
        handleDrag() {
            // 如果不需要可以移除
        },
        handleClickClass(classId) {
            this.$router.push({
                path: `/class/class-detail/${classId}`
            })
        }
    }
}
</script>
  
<style scoped>
.mixin-components-container {
    background-color: #f0f2f5;
    padding: 30px;
    min-height: calc(100vh - 84px);
}

.component-item {
    min-height: 100px;
}
</style>
  