﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SeeSharpTools.JY.GUI.TabCursorUtility
{
    public partial class TabCursorInfoForm : Form
    {
        private readonly TabCursorCollection _cursors;
        // 当前是否是内部更新表格操作的标识，避免内部修改值时导致的事件递归。如果为true则不执行响应事件；如果为false是外部用户输入，执行响应事件
        private bool _isInternalOperation;
        private readonly EasyChartX _parentChart;

        public TabCursorInfoForm(EasyChartX parentChart)
        {
            InitializeComponent();
            this._cursors = parentChart.TabCursors;
            this._parentChart = parentChart;
            RefreshCursorInfo();
            SetFormLocation(parentChart);
            const string formNameFormat = "{0} Tab Cursors";
            this.Text = string.Format(formNameFormat, parentChart.Name);
            this._isInternalOperation = false;

            this._parentChart.TabCursorChanged += RefreshCursorValue;
        }

        private void SetFormLocation(EasyChartX parentChart)
        {
            Control currentControl = parentChart;
            int chartAbsoluteX = parentChart.Location.X;
            int chartAbsoluteY = parentChart.Location.X;
            while (null != currentControl.Parent)
            {
                currentControl = currentControl.Parent;
                chartAbsoluteX += currentControl.Location.X;
                chartAbsoluteY += currentControl.Location.Y;
            }
            int formPositionX = chartAbsoluteX + parentChart.Width - this.Width/2;
            int formPositionY = chartAbsoluteY + parentChart.Height - this.Height;
            Rectangle workingArea = Screen.GetWorkingArea(this);
            if (formPositionX + this.Width > workingArea.Width + workingArea.X)
            {
                formPositionX = workingArea.Width + workingArea.X - this.Width;
            }
            else if (formPositionX < 0)
            {
                formPositionX = 0;
            }

            if (formPositionY + this.Height > workingArea.Height + workingArea.Y)
            {
                formPositionY = workingArea.Height + workingArea.Y - this.Height;
            }
            else if (formPositionY < 0)
            {
                formPositionY = 0;
            }
            this.Location = new Point(formPositionX, formPositionY);
        }

        const int CursorEnableIndex = 0;
        const int CursorColorIndex = 1;
        const int CursorNameIndex = 2;
        const int CursorValueIndex = 3;
        const string ColorButtonText = "Select";

        private void button_add_Click(object sender, EventArgs e)
        {
            _cursors.Add(new TabCursor());
            RefreshCursorInfo();
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            if (0 == dataGridView_cursorInfo.SelectedCells.Count)
            {
                return;
            }
            HashSet<int> rowIndexes = new HashSet<int>();
            foreach (DataGridViewCell selectCell in dataGridView_cursorInfo.SelectedCells)
            {
                rowIndexes.Add(selectCell.RowIndex);
            }
            foreach (int rowIndex in rowIndexes)
            {
                string cursorName = dataGridView_cursorInfo.Rows[rowIndex].Cells[CursorNameIndex].Value.ToString();
                TabCursor deleteCursor = _cursors.First(item => item.Name.Equals(cursorName));
                if (null != deleteCursor)
                {
                    _cursors.Remove(deleteCursor);
                }
            }
            RefreshCursorInfo();
        }

        private void RefreshCursorInfo()
        {
            dataGridView_cursorInfo.Rows.Clear();
            foreach (TabCursor cursor in _cursors)
            {
                dataGridView_cursorInfo.Rows.Add(cursor.Enabled, ColorButtonText, cursor.Name, cursor.Value);
                DataGridViewRow row = dataGridView_cursorInfo.Rows[dataGridView_cursorInfo.Rows.Count-1];
                row.Cells[CursorColorIndex].Style.BackColor = cursor.Color;
                row.Cells[CursorColorIndex].Style.SelectionBackColor = cursor.Color;
            }
        }

        private void RefreshCursorValue(object sender, TabCursorEventArgs eventArgs)
        {
            // 如果只有数据更新或者使能更新，则只刷新
            if (eventArgs.Operation == TabCursorOperation.ValueChanged)
            {
                int index = _cursors.IndexOf(eventArgs.Cursor);
                dataGridView_cursorInfo.Rows[index].Cells[CursorEnableIndex].Value = _cursors[index].Enabled;
                dataGridView_cursorInfo.Rows[index].Cells[CursorColorIndex].Style.BackColor = _cursors[index].Color;
                dataGridView_cursorInfo.Rows[index].Cells[CursorNameIndex].Value = _cursors[index].Name;
                dataGridView_cursorInfo.Rows[index].Cells[CursorValueIndex].Value = _cursors[index].Value;
            }
            else
            {
                RefreshCursorInfo();
            }
            
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            _cursors.Clear();
            dataGridView_cursorInfo.Rows.Clear();
        }

        private void dataGridView_cursorInfo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isInternalOperation || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            _isInternalOperation = true;
            object changedValue = dataGridView_cursorInfo.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            TabCursor changeCursor = _cursors[e.RowIndex];
            switch (e.ColumnIndex)
            {
//                case CursorEnableIndex:
//                    changeCursor.Enabled = (bool)changedValue;
//                    break;
//                case CursorColorIndex:
//                    break;
                case CursorNameIndex:
                    if (null != changedValue && _cursors.All(cursor => !cursor.Name.Equals(changedValue)))
                    {
                        changeCursor.Name = changedValue.ToString();
                    }
                    dataGridView_cursorInfo.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = changeCursor.Name;
                    break;
                case CursorValueIndex:
                    double xValue;
                    if (double.TryParse(changedValue.ToString(), out xValue))
                    {
                        changeCursor.Value = xValue;
                    }
                    dataGridView_cursorInfo.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = changeCursor.Value;
                    break;
                default:
                    break;
            }
            _isInternalOperation = false;
        }

        private void dataGridView_cursorInfo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_isInternalOperation || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            _isInternalOperation = true;
            TabCursor changeCursor = _cursors[e.RowIndex];
            switch (e.ColumnIndex)
            {
                case CursorEnableIndex:
//                    dataGridView_cursorInfo.EndEdit();
                    changeCursor.Enabled = !changeCursor.Enabled;
                    dataGridView_cursorInfo.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = changeCursor.Enabled;
//                    dataGridView_cursorInfo.EndEdit();
                    break;
                case CursorColorIndex:
                    ColorDialog loColorForm = new ColorDialog();
                    if (loColorForm.ShowDialog() == DialogResult.OK)
                    {
                        changeCursor.Color = loColorForm.Color;
                        DataGridViewRow row = dataGridView_cursorInfo.Rows[e.RowIndex];
                        row.Cells[CursorColorIndex].Style.BackColor = changeCursor.Color;
                        row.Cells[CursorColorIndex].Style.SelectionBackColor = changeCursor.Color;
                    }
                    break;
//                case CursorNameIndex:
//                    break;
//                case CursorValueIndex:
//                    break;
                default:
                    break;
            }
            _isInternalOperation = false;
        }

        // 窗体失去焦点时自动提交对用户对值的修改
        private void DynamicCursorInfoForm_Deactivate(object sender, EventArgs e)
        {
            dataGridView_cursorInfo.EndEdit();
        }

        private void DynamicCursorInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._parentChart.TabCursorChanged -= RefreshCursorValue;
        }
    }
}
